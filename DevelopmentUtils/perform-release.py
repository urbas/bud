#!/usr/bin/env python

import sys
import re
import semver
from argparse import ArgumentParser
from subprocess import call
from os.path import join
from tempfile import mkstemp
from shutil import move
from os import remove, close, getcwd, chdir



BUD_BUILD_CS='.bud/Build.cs'
BUD_VERSION_CS='Bud.Core/src/main/cs/BudVersion.cs'
BUD_ASSEMBLY_INFO_CS='bud/src/main/cs/Properties/AssemblyInfo.cs'
BUD_CORE_ASSEMBLY_INFO_CS='Bud.Core/src/main/cs/Properties/AssemblyInfo.cs'
CHOCOLATEY_PACKAGE_DIR='DevelopmentUtils/ChocolateyPackage'
CHOCOLATEY_NUSPEC_FILE=join(CHOCOLATEY_PACKAGE_DIR, 'bud.nuspec')
CHOCOLATEY_PS1_FILE=join(CHOCOLATEY_PACKAGE_DIR, 'tools', 'chocolateyInstall.ps1')
BUD_DIST_DIR=join('bud', '.bud', 'output', 'main', 'cs', 'dist')



class BudInfo:
  bud_exe_dir=None


def perform_release(version):
  update_version(version)
  git_tag_release(version)
  publish(version)
  next_dev_version=set_next_dev_version(version)
  git_commit_next_dev(next_dev_version)


def set_next_dev_version(version):
  v=semver.parse(version)
  next_dev_version='{0}.{1}.{2}-dev'.format(v['major'], v['minor'], v['patch']+1)
  update_version(next_dev_version)
  return next_dev_version


def update_version(version):
  update_build_cs_version(version)
  update_bud_sources_version(version)
  update_nuspec_version(version)
  update_dist_zip_url(version)


def git_tag_release(version):
  git_release_commit(version)
  git_tag_release(version)


def publish(version):
  publish_to_nuget()
  create_dist()
  create_zip(version)
  upload_package(version)
  chocolatey_push(version)


def transform_lines(file_path, transformation_callback):
    fh, abs_path = mkstemp()
    with open(abs_path,'w') as new_file:
        with open(file_path) as old_file:
            for line in old_file:
                new_file.write(transformation_callback(line))
    close(fh)
    remove(file_path)
    move(abs_path, file_path)


def replace_lines(file_path, pattern, subst):
  transform_lines(file_path, lambda line: re.sub(pattern, subst, line))


def replace_in_file(file, regex_s_replacement):
  call(['sed', '-r', regex_s_replacement, '-i', file])


def update_build_cs_version(version):
  replace_lines(BUD_BUILD_CS, r'\.Version\(.*?\)', '.Version("{0}")'.format(version))


def update_assembly_info_version(version, assembly_info_file):
  semantic_version = semver.parse(version)
  replace_lines(assembly_info_file, r'(AssemblyVersion\s*\(").*?("\))', r'\g<1>{0}.{1}.{2}\g<2>'.format(semantic_version['major'], semantic_version['minor'], semantic_version['patch']))
  replace_lines(assembly_info_file, r'(AssemblyDescription\s*\(").*?("\))', r'\g<1>{0}\g<2>'.format(version))


def update_bud_sources_version(version):
  replace_lines(BUD_VERSION_CS, r'Current = ".*?";', 'Current = "{0}";'.format(version))
  update_assembly_info_version(version, BUD_ASSEMBLY_INFO_CS)
  update_assembly_info_version(version, BUD_CORE_ASSEMBLY_INFO_CS)


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


def git_commit_next_dev(version):
  call(['git', 'commit', '-am', 'Setting next development version to {0}'.format(version)])


def bud_exe():
  if BudInfo.bud_exe_dir == None:
    return 'bud.exe'
  return join(BudInfo.bud_exe_dir, 'bud.exe')


def create_dist():
  call([bud_exe(), 'project/bud/clean'])
  call([bud_exe(), 'project/bud/main/cs/dist'])


def publish_to_nuget():
  call([bud_exe(), 'publish'])


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


def run():
  parser = ArgumentParser(description="Prepares Bud for the release")
  parser.add_argument('version', metavar='VERSION', nargs=1, help="Version of Bud being released")
  parser.add_argument('--bud-dir', help="the directory containing Bud's executable")
  args = parser.parse_args(sys.argv[1:])
  BudInfo.bud_exe_dir = args.bud_dir
  perform_release(args.version[0])


if __name__ == "__main__":
  run()