import os
import re
import shutil
import subprocess
import sys

PUSH = False

PROJECT_NAME = "shared-library"
GIT_MAIN_BRANCH = "main"

PYTHON_FILE_FOLDER = os.path.dirname(os.path.realpath(__file__))
PROJECT_FOLDER = os.path.join(PYTHON_FILE_FOLDER, "src", PROJECT_NAME)
BUILD_FOLDER = os.path.join(PYTHON_FILE_FOLDER, "build")

BUILD_VERSION = "0.0.0"

NUGET_IMAGE_NAME = "phis.shared-library"
NUGET_SOURCE = "https://nuget.theaurum.net/v3/index.json"
NUGET_PASSWORD = ""

DOCKER_REGISTRY = "docker-registry.theaurum.net"
DOCKER_REGISTRY_USERNAME = ""
DOCKER_REGISTRY_PASSWORD = ""

# region print

class bcolors:
    BLUE = '\033[94m'
    CYAN = '\033[96m'
    GREEN = '\033[92m'
    WARNING = '\033[93m'
    RED = '\033[91m'
    END = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'


def print_header(text_to_print):
    print(f"\n{bcolors.BLUE}{bcolors.BOLD}{bcolors.UNDERLINE}"
          f"=========================================================\n"
          + text_to_print
          + "\n========================================================="
          + bcolors.END,
          flush=True)


def print_green(text_to_print):
    print(bcolors.GREEN + text_to_print + bcolors.END, flush=True)


def print_red(text_to_print):
    print(bcolors.RED + text_to_print + bcolors.END, flush=True)


# endregion

def run_command(command, should_print=False, working_directory=PYTHON_FILE_FOLDER):
    command_as_string = " ".join(command)

    print_green(f"Executing command \"{command_as_string}\"")
    if working_directory is not PYTHON_FILE_FOLDER:
        print_green("Working directory: \"" + working_directory + "\"")

    p = subprocess.Popen(
        command,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        universal_newlines=True,
        cwd=working_directory)

    out = ""

    while p.poll() is None:
        line = p.stdout.readline()  # This blocks until it receives a newline.
        if not line:
            continue
        if should_print:
            print_green(line.rstrip())
        out += line

    p.communicate()

    if p.returncode != 0:
        raise Exception(f"Error while trying to execute command: \"{command_as_string}\"")

    return out


# region manage_args

def manage_arg_BUILD_VERSION(args, arg_position):
    global BUILD_VERSION
    BUILD_VERSION = args[arg_position + 1]
    if not re.search("^\d{0,3}.\d{0,3}.\d{0,3}$", BUILD_VERSION):
        raise Exception(
            f"{BUILD_VERSION=} is not valid. Valid versions examples: \"123.123.123\", \"1.2.3\", \"1.2.123\"")

def manage_arg_NUGET_PASSWORD(args, arg_position):
    global NUGET_PASSWORD
    NUGET_PASSWORD = args[arg_position + 1]
    if not NUGET_PASSWORD:
        raise Exception(f"{NUGET_PASSWORD=} is not valid")


def manage_args(args):
    if len(args) <= 0:
        print_header("No args given. Using dev build")

    print(args)

    global PUSH
    PUSH = False
    build_version_set = False
    nuget_password_set = False

    for i in range(len(args)):
        arg = args[i]

        if arg.lower() == "-PUSH".lower():
            if PUSH:
                raise Exception("\"-PUSH\" is already defined")
            PUSH = True

        if arg.lower() == "-BUILD_VERSION".lower():
            if build_version_set:
                raise Exception("\"-BUILD_VERSION\" is already defined")
            manage_arg_BUILD_VERSION(args, i)
            build_version_set = True

        if arg.lower() == "-NUGET_PASSWORD".lower():
            if nuget_password_set:
                raise Exception("\"-NUGET_PASSWORD\" is already defined")
            manage_arg_NUGET_PASSWORD(args, i)
            nuget_password_set = True

    if PUSH:
        missing_args = False
        if not build_version_set:
            print_red("If \"-PUSH\" is set, \"-BUILD_VERSION\" also needs to be set")
            missing_args = True

        if not nuget_password_set:
            print_red("If \"-PUSH\" is set, \"-NUGET_PASSWORD\" also needs to be set")
            missing_args = True

        if missing_args:
            raise Exception("\"-PUSH\" is set but some required args are missing")

    print_green("\n============================================================")
    print_green(f"{PROJECT_NAME=}")
    print_green(f"{PYTHON_FILE_FOLDER=}")
    print_green(f"{BUILD_FOLDER=}")
    print_green(f"{BUILD_VERSION=}")
    print_green(f"{NUGET_SOURCE=}")
    print_green(f"{PUSH=}")
    if PUSH:
        print_green(f"{NUGET_PASSWORD=}")
    print_green("============================================================\n")


# endregion

# region build
def create_nuget_package():
    shutil.rmtree(BUILD_FOLDER, ignore_errors=True)
    run_command(["dotnet", "pack",
                 "-c", "Release",
                 f"/p:Version={BUILD_VERSION}",
                 "-o", BUILD_FOLDER],
                should_print=True,
                working_directory=os.path.join(PROJECT_FOLDER))


# endregion

# region PUSH

def check_if_pending_changes():
    result = run_command(["git", "status", "--porcelain"], should_print=True)
    if result:
        raise Exception("Project got pending changes")


def switching_to_main_branch():
    run_command(["git", "fetch", "origin", "-v"], should_print=True)
    run_command(["git", "switch", "-f", GIT_MAIN_BRANCH], should_print=True)
    run_command(["git", "reset", "--hard", "HEAD"], should_print=True)
    run_command(["git", "pull"], should_print=True)
    run_command(["git", "clean", "-d", "-f"], should_print=True)


def push_to_nuget():
    run_command(["dotnet", "nuget", "push",
                 "-s", NUGET_SOURCE,
                 "-k", NUGET_PASSWORD,
                 os.path.join(BUILD_FOLDER, NUGET_IMAGE_NAME),
                 "--skip-duplicate"],
                should_print=True)


def push_to_git():
    shutil.rmtree(BUILD_FOLDER)
    run_command(["git", "tag", "-a", f"v{BUILD_VERSION}", "-m",
                 f"v{BUILD_VERSION}\n"
                 f"{NUGET_IMAGE_NAME}"],
                should_print=True)
    run_command(["git", "push", "--tags"], should_print=True)


# endregion

def set_variables():
    global NUGET_IMAGE_NAME
    NUGET_IMAGE_NAME = f"{NUGET_IMAGE_NAME}.{BUILD_VERSION}.nupkg"


def main():
    try:
        os.system("")  # allows colours to be printed in Command Prompt and Powershell natively
        print_header("Build starting...")
        manage_args(sys.argv[1:])
        set_variables()

        if PUSH:
            print_header("Checking if project got pending changes")
            check_if_pending_changes()
            print_header("Switching to main branch")
            switching_to_main_branch()

        print_header(f"Creating nuget package from common project")
        create_nuget_package()

        if PUSH:
            print_header("Pushing build results")
            print_header(f"Pushing common nuget package to nuget server")
            push_to_nuget()

            print_header("Pushing build tag to git")
            push_to_git()
        print_green("========== SUCCESSFUL ===========")
    except Exception as e:
        print_red("========== FAILED ===========\n" + str(e))


if __name__ == "__main__":
    main()
