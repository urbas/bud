#!/usr/bin/env python

import sys
import os
from argparse import ArgumentParser
from subprocess import call
from os.path import join

CHOCOLATEY_PACKAGE_DIR='DevelopmentUtils/ChocolateyPackage'
CHOCOLATEY_NUSPEC_FILE=join(CHOCOLATEY_PACKAGE_DIR, 'bud.nuspec')
CHOCOLATEY_PS1_FILE=join(CHOCOLATEY_PACKAGE_DIR, 'tools', 'chocolateyInstall.ps1')
BUD_DIST_DIR=join('bud', '.bud', 'output', 'main', 'cs', 'dist')


def replace_in_file(file, regex_s_replacement):
  call(['sed', '-r', regex_s_replacement, '-i', file])


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


def bud_dist_zip_name(version):
  return 'bud-{0}.zip'.format(version)


def bud_choco_package_name(version):
  return 'bud.{0}.nupkg'.format(version)


def create_zip(version):
  old_dir=os.getcwd()
  os.chdir(BUD_DIST_DIR)
  call(['zip', '-r', bud_dist_zip_name(version), '.'])
  os.chdir(old_dir)


def chocolatey_push(version):
  old_dir=os.getcwd()
  os.chdir(CHOCOLATEY_PACKAGE_DIR)
  call(['cpack'])
  call(['cpush', bud_choco_package_name(version)])
  os.chdir(old_dir)


def upload_package(version):
  package_name=bud_dist_zip_name(version)
  package_relative_path=join('.', BUD_DIST_DIR, package_name)
  call(['scp', '-i', '~/.ssh/budpage_id_rsa', package_relative_path, 'budpage@54.154.215.159:/home/budpage/production-budpage/shared/public/packages/{0}'.format(package_name)])


def prepare_release(version):
  update_nuspec_version(version)
  update_dist_zip_url(version)
  git_release_commit(version)
  git_tag_release(version)
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