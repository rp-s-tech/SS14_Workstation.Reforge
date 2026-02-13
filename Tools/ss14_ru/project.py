import pathlib
import os
import glob
import subprocess
import sys
from file import FluentFile

class Project:
    def __init__(self):
        script_dir = pathlib.Path(__file__).resolve().parent
        self.source_dir_path = script_dir.parent.parent.resolve()
        self.workspace_dir_path = self.source_dir_path.parent.resolve()

        target_dir = self.workspace_dir_path / "RPS.SS14.NewWorld"
        if target_dir.is_dir():
            self.base_dir_path = target_dir
        else:
            # Fallback to legacy behavior if target repo is absent.
            self.base_dir_path = self.source_dir_path

        self.resources_dir_path = os.path.join(str(self.base_dir_path), 'Resources')
        self.locales_dir_path = os.path.join(self.resources_dir_path, 'Locale')
        self.ru_locale_dir_path = os.path.join(self.locales_dir_path, 'ru-RU')
        self.en_locale_dir_path = os.path.join(self.locales_dir_path, 'en-US')
        self.ru_primary_layer_dir_path = os.path.join(self.ru_locale_dir_path, 'rpsx')
        self.ru_base_layer_dir_path = os.path.join(self.ru_locale_dir_path, 'space-axolotl-14')
        self.prototypes_dir_path = os.path.join(self.resources_dir_path, "Prototypes")
        self.en_locale_prototypes_dir_path = os.path.join(self.en_locale_dir_path, 'ss14-ru', 'prototypes')
        self.ru_locale_prototypes_dir_path = os.path.join(self.ru_primary_layer_dir_path, 'ss14-ru', 'prototypes')

    def get_files_paths_by_dir(self, dir_path, files_extenstion):
        return glob.glob(f'{dir_path}/**/*.{files_extenstion}', recursive=True)

    def get_fluent_files_by_dir(self, dir_path):
        files = []
        files_paths_list = glob.glob(f'{dir_path}/**/*.ftl', recursive=True)

        for file_path in files_paths_list:
            try:
                files.append(FluentFile(file_path))
            except:
                continue

        return files

    def get_ru_primary_fluent_file_path(self, en_file_path):
        relative_path = os.path.relpath(en_file_path, self.en_locale_dir_path)
        return os.path.join(self.ru_primary_layer_dir_path, relative_path)

    def get_ru_base_fluent_file_path(self, en_file_path):
        relative_path = os.path.relpath(en_file_path, self.en_locale_dir_path)
        return os.path.join(self.ru_base_layer_dir_path, relative_path)

    @staticmethod
    def is_path_in_directory(file_path, directory_path):
        file_path_abs = os.path.abspath(file_path)
        directory_path_abs = os.path.abspath(directory_path)
        try:
            return os.path.commonpath([file_path_abs, directory_path_abs]) == directory_path_abs
        except ValueError:
            return False

    def get_ru_non_primary_fluent_files(self):
        all_ru_files = self.get_fluent_files_by_dir(self.ru_locale_dir_path)
        return list(filter(
            lambda file: not Project.is_path_in_directory(file.full_path, self.ru_primary_layer_dir_path),
            all_ru_files
        ))


def run_translation_pipeline():
    script_dir = pathlib.Path(__file__).resolve().parent
    requirements_path = script_dir / "requirements.txt"
    print("[ss14_ru] install dependencies")
    install = subprocess.run(
        [sys.executable, "-m", "pip", "install", "-r", str(requirements_path), "--no-warn-script-location"],
        cwd=str(script_dir)
    )
    if install.returncode != 0:
        raise SystemExit(install.returncode)
    upgrade_typing_extensions = subprocess.run(
        [sys.executable, "-m", "pip", "install", "--upgrade", "typing_extensions", "--no-warn-script-location"],
        cwd=str(script_dir)
    )
    if upgrade_typing_extensions.returncode != 0:
        raise SystemExit(upgrade_typing_extensions.returncode)

    steps = [
        "yamlextractor.py",
        "keyfinder.py",
        "clean_duplicates.py",
        "clean_empty.py",
    ]

    for step in steps:
        step_path = script_dir / step
        print(f"[ss14_ru] run: {step}")
        result = subprocess.run([sys.executable, str(step_path)], cwd=str(script_dir))
        if result.returncode != 0:
            raise SystemExit(result.returncode)


if __name__ == "__main__":
    run_translation_pipeline()

