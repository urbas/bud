#!/usr/bin/env python

import sys
from argparse import ArgumentParser
from subprocess import call
from os.path import join

CHOCOLATEY_PACKAGE_DIR='DevelopmentUtils/ChocolateyPackage'
CHOCOLATEY_NUSPEC_FILE=join(CHOCOLATEY_PACKAGE_DIR, 'bud.nuspec')
CHOCOLATEY_PS1_FILE=join(CHOCOLATEY_PACKAGE_DIR, 'tools', 'chocolateyInstall.ps1')

def replace_in_file(file, regex_s_replacement):
  call(['sed', '-r', regex_s_replacement, '-i', file])


def update_nuspec_version(version):
  replacement='s#<version>.+</version>#<version>{0}</version>#'.format(version)
  replace_in_file(CHOCOLATEY_NUSPEC_FILE, replacement)


def update_dist_zip_url(version):
  replacement='s/bud-.+?\\.zip/bud-{0}.zip/'.format(version)
  replace_in_file(CHOCOLATEY_PS1_FILE, replacement)


def git_release_commit(version):
  call(['git', 'commit', '-am', 'Release {0}'.format(version)])


def git_tag_release(version):
  call(['git', 'tag', 'v{0}'.format(version)])


def prepare_release(version):
  update_nuspec_version(version)
  update_dist_zip_url(version)
  git_release_commit(version)
  git_tag_release(version)


def run():
  parser = ArgumentParser(description="Prepares Bud for the release")
  parser.add_argument('version', metavar='VERSION', nargs=1, help="Version of Bud being released.")
  args = parser.parse_args(sys.argv[1:])
  prepare_release(args.version[0])


if __name__ == "__main__":
  run()