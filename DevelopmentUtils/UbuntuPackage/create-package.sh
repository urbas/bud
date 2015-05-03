#!/bin/bash

set -e

# Sample usage: BUD_DIST_DIR=~/.bud/dist/v0.3.0/ BUD_VERSION=0.3.0 ./create-package.sh

BUD_DIST_DIR="${BUD_DIST_DIR?You have to provide the distribution directory of Bud.}"
BUD_VERSION="${BUD_VERSION?You have to provide the version.}"

__BUD_DIR=bud-$BUD_VERSION
__DEB_DIR=$__BUD_DIR/debian

rm -Rf bud*

mkdir $__BUD_DIR

cp -R template/debian $__BUD_DIR
cp -R template/usr $__BUD_DIR
cp template/bud_x.y.z.orig.tar.xz bud_$BUD_VERSION.orig.tar.xz
mkdir -p $__BUD_DIR/usr/lib/bud 
cp -R $BUD_DIST_DIR/* $__BUD_DIR/usr/lib/bud

pushd $__BUD_DIR

find usr > debian/source/include-binaries
dpkg-buildpackage -b

popd
