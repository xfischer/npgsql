#!/bin/bash

DOTNET_STAGING_PATH=$1
EDB_INSTALLBUILDER_BIN=$2
GIT_REPO_PATH=$3
REDUX_TAG_NAME=$4

###################### 
# Get REDUX repository
######################

rm -rf $PWD/redux

git clone $GIT_REPO_PATH/REDUX redux

pushd redux
git checkout "$REDUX_TAG_NAME"
popd

# Source redux files
source $PWD/redux/common/branding.sh
source $PWD/redux/common/redux-helperfunctions.sh
source $PWD/pkg/dotnet_branding.sh

# Get .NET version
source $DOTNET_STAGING_PATH/dotnet-version.sh

PackageInstaller "dotnet" "$DOTNET_STAGING_PATH" "DOTNET_INSTALLER_NAME_PREFIX" "EDB_VERSION_DOTNET" "EDB_BUILDNUM_DOTNET" "$PWD/redux"
