#!/usr/bin/env python

import sys
import re
from argparse import ArgumentParser
from subprocess import call
from os.path import join
from tempfile import mkstemp
from shutil import move
from os import remove, close, getcwd, chdir



BUD_BUILD_CS='.bud/Build.cs'
CHOCOLATEY_PACKAGE_DIR='DevelopmentUtils/ChocolateyPackage'
CHOCOLATEY_NUSPEC_FILE=join(CHOCOLATEY_PACKAGE_DIR, 'bud.nuspec')
CHOCOLATEY_PS1_FILE=join(CHOCOLATEY_PACKAGE_DIR, 'tools', 'chocolateyInstall.ps1')
BUD_DIST_DIR=join('bud', '.bud', 'output', 'main', 'cs', 'dist')


def replace_lines(file_path, pattern, subst):
    fh, abs_path = mkstemp()
    with open(abs_path,'w') as new_file:
        with open(file_path) as old_file:
            for line in old_file:
                new_file.write(re.sub(pattern, subst, line))
    close(fh)
    remove(file_path)
    move(abs_path, file_path)


def replace_in_file(file, regex_s_replacement):
  call(['sed', '-r', regex_s_replacement, '-i', file])


def update_bud_version(version):
  replace_lines(BUD_BUILD_CS, r'\.Version\(.*?\)', '.Version("{0}")'.format(version))


def update_nuspec_version(version):
  replacement='s#<version>.+</version>#<version>{0}</version>#'.format(version)
  replace_in_file(CHOCOLATEY_NUSPEC_FILE, replacement)


def update_dist_zip_url(version):
  replacement='s/bud-.+?\\.zip/{0}/'.format(bud_dist_zip_name(version))
  replace_in_file(CHOCOLATEY_PS1_FILE, replacement)


def git_release_commit(version):
  call(['git', 'commit', '-am', 'Release {0}'.format(version)])


def git_tag_release(version):
  call(['git', 'tag', 'v{0}'.format(version)])


def create_dist():
  call(['bud', 'project/bud/clean'])
  call(['bud', 'project/bud/main/cs/dist'])


def publish_to_nuget():
  call(['bud', 'publish'])


def bud_dist_zip_name(version):
  return 'bud-{0}.zip'.format(version)


def bud_choco_package_name(version):
  return 'bud.{0}.nupkg'.format(version)


def create_zip(version):
  old_dir=getcwd()
  chdir(BUD_DIST_DIR)
  call(['zip', '-r', bud_dist_zip_name(version), '.'])
  chdir(old_dir)


def chocolatey_push(version):
  old_dir=getcwd()
  chdir(CHOCOLATEY_PACKAGE_DIR)
  call(['cpack'])
  call(['cpush', bud_choco_package_name(version)])
  chdir(old_dir)


def upload_package(version):
  package_name=bud_dist_zip_name(version)
  package_relative_path=join('.', BUD_DIST_DIR, package_name)
  call(['scp', '-i', '~/.ssh/budpage_id_rsa', package_relative_path, 'budpage@54.154.215.159:/home/budpage/production-budpage/shared/public/packages/{0}'.format(package_name)])


def prepare_release(version):
  update_bud_version(version)
  update_nuspec_version(version)
  update_dist_zip_url(version)

  git_release_commit(version)
  git_tag_release(version)

  publish_to_nuget()
  
  create_dist()
  create_zip(version)
  upload_package(version)
  chocolatey_push(version)


def run():
  parser = ArgumentParser(description="Prepares Bud for the release")
  parser.add_argument('version', metavar='VERSION', nargs=1, help="Version of Bud being released.")
  args = parser.parse_args(sys.argv[1:])
  prepare_release(args.version[0])


if __name__ == "__main__":
  run()