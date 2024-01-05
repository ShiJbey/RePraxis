#!/usr/bin/python3

"""Re:Praxis Release Bundler

This script assists with creating proper bundles for release

"""
import pathlib
import shutil
import subprocess
import xml.etree.ElementTree as ET

PROJECT_ROOT = pathlib.Path(__file__).parent

BUILD_DIR = PROJECT_ROOT / "dist" / "RePraxis"

LICENSE_PATH = PROJECT_ROOT / "LICENSE.md"

README_PATH = PROJECT_ROOT / "README.md"

CSPROJ_PATH = PROJECT_ROOT / "src" / "RePraxis" / "RePraxis.csproj"


def get_project_version() -> str:
    """Read the project version number from the csproj file."""
    tree = ET.parse(CSPROJ_PATH)

    root = tree.getroot()

    try:
        version_elem = root.findall(".//Version")[0]
    except IndexError:
        raise Exception(f"Could not find <Version> element in: {CSPROJ_PATH}.")

    version_text = version_elem.text

    if not isinstance(version_text, str):
        raise TypeError(f"Version element in '{CSPROJ_PATH}' missing inner text.")

    return version_text


def main():
    # Create a new build
    completed_proc = subprocess.run(["dotnet","build","--configuration","Release"])

    if completed_proc.returncode != 0:
        print("An error occurred during build")

    # Copy the license and readme to the built distribution
    shutil.copyfile(LICENSE_PATH, BUILD_DIR / "LICENSE.md")
    shutil.copyfile(README_PATH, BUILD_DIR / "README.md")

    # Zip the build directory
    project_version = get_project_version()
    shutil.make_archive(f"dist/RePraxis_{project_version}", "zip", BUILD_DIR)


if __name__ == "__main__":
    main()
