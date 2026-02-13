import os
from datetime import datetime
from fluent.syntax import ast

from file import FluentFile
from fluentast import FluentAstAbstract
from project import Project


def get_ftl_files(root_dir):
    files = []
    for root, _, file_names in os.walk(root_dir):
        for name in file_names:
            if name.endswith(".ftl"):
                files.append(os.path.join(root, name))
    return files


def get_message_ids(resource):
    ids = set()
    for element in resource.body:
        if isinstance(element, ast.ResourceComment) or isinstance(element, ast.GroupComment) or isinstance(element, ast.Comment):
            continue

        element_id = FluentAstAbstract.get_id_name(element)
        if element_id:
            ids.add(element_id)

    return ids


def is_comment_or_junk(element):
    return isinstance(element, ast.ResourceComment) or \
        isinstance(element, ast.GroupComment) or \
        isinstance(element, ast.Comment) or \
        isinstance(element, ast.Junk)


def build_other_ru_key_index(project: Project):
    other_ru_key_index = {}
    other_ru_files = project.get_ru_non_primary_fluent_files()

    for other_ru_file in other_ru_files:
        try:
            other_ru_parsed = other_ru_file.parse_data(other_ru_file.read_data())
        except Exception:
            continue

        for message_id in get_message_ids(other_ru_parsed):
            if message_id not in other_ru_key_index:
                other_ru_key_index[message_id] = set()
            other_ru_key_index[message_id].add(other_ru_file.full_path)

    return other_ru_key_index


def remove_cross_layer_duplicates(project: Project):
    removed_duplicates = []
    modified_files = 0
    rpsx_files = get_ftl_files(project.ru_primary_layer_dir_path)
    other_ru_key_index = build_other_ru_key_index(project)

    for rpsx_file_path in rpsx_files:
        rpsx_file = FluentFile(rpsx_file_path)

        try:
            rpsx_parsed = rpsx_file.parse_data(rpsx_file.read_data())
        except Exception:
            continue

        new_body = []
        file_changed = False

        for element in rpsx_parsed.body:
            if is_comment_or_junk(element):
                new_body.append(element)
                continue

            element_id = FluentAstAbstract.get_id_name(element)
            if not element_id:
                new_body.append(element)
                continue

            if element_id not in other_ru_key_index:
                new_body.append(element)
                continue

            file_changed = True
            for other_file_path in sorted(other_ru_key_index[element_id]):
                removed_duplicates.append((element_id, rpsx_file_path, other_file_path))

        if file_changed:
            rpsx_parsed.body = new_body
            rpsx_file.save_data(rpsx_file.serialize_data(rpsx_parsed))
            modified_files += 1

    return removed_duplicates, modified_files


def write_log(duplicates, modified_files):
    log_filename = f"duplicates_report_{datetime.now().strftime('%Y%m%d_%H%M%S')}.log"
    with open(log_filename, "w", encoding="utf-8") as log_file:
        if not duplicates:
            log_file.write("Дубликаты между ru-RU/rpsx и другими слоями ru-RU не найдены. Удалений нет.\n")
            return log_filename

        unique_duplicates = sorted(set(duplicates))
        log_file.write("Удалены дубли ключей из ru-RU/rpsx (ключ уже существует в другом слое ru-RU):\n\n")
        log_file.write(f"Изменено файлов rpsx: {modified_files}\n")
        log_file.write(f"Удалено дублей (уникальных): {len(unique_duplicates)}\n\n")
        for message_id, rpsx_file_path, base_file_path in unique_duplicates:
            log_file.write(f'Ключ: {message_id}\n')
            log_file.write(f'  rpsx: {rpsx_file_path}\n')
            log_file.write(f'  other: {base_file_path}\n\n')

    return log_filename


if __name__ == "__main__":
    project = Project()
    duplicates, modified_files = remove_cross_layer_duplicates(project)
    log_file = write_log(duplicates, modified_files)

    print(f"Проверено файлов слоя rpsx: {len(get_ftl_files(project.ru_primary_layer_dir_path))}")
    print(f"Удалено дублей: {len(set(duplicates))}")
    print(f"Изменено файлов rpsx: {modified_files}")
    print(f"Лог сохранен: {log_file}")
