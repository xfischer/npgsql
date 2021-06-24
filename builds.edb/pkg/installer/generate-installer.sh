#!/bin/bash

DOTNET_STAGING_PATH=$1
EDB_INSTALLBUILDER_BIN=$2

# Source redux files
source $PWD/redux/common/branding.sh
source $PWD/redux/common/redux-helperfunctions.sh
source $PWD/pkg/dotnet_branding.sh

# Get .NET version
source $DOTNET_STAGING_PATH/dotnet-version.sh

ReplacePlaceHolders(){

	filepath=$1

	Message $MSG_INFO "Replacing placeholders in $filepath"

	# Replace placeholders
	Replace EDB_MAIN_MENU "$EDB_MAIN_MENU" $filepath
	Replace EDB_BRANDING "$EDB_BRANDING" $filepath
	Replace REDUX_REPO_PATH "$REDUX_REPO_PATH" $filepath

	# .NET
	Replace DOTNET_SHORT_NAME $DOTNET_SHORT_NAME $filepath
	Replace EDB_VERSION_DOTNET $EDB_VERSION_DOTNET $filepath
	Replace EDB_BUILDNUM_DOTNET $EDB_BUILDNUM_DOTNET $filepath
	Replace DOTNET_INSTALLER_NAME_PREFIX $DOTNET_INSTALLER_NAME_PREFIX $filepath
	Replace DOTNET_INSTALL_DIR $DOTNET_INSTALL_DIR $filepath
	Replace DOTNET_STAGING_PATH $DOTNET_STAGING_PATH $filepath
	Replace NET45_FRAMEWORK $NET45_FRAMEWORK $filepath
	Replace NET50_FRAMEWORK $NET50_FRAMEWORK $filepath
}

if [[ $OS == *"Msys"* ]]; then

	REDUX_REPO_PATH=$(echo $PWD/redux | sed 's/^\///' | sed 's/\//\\\\/g' | sed 's/^./\0:/')
fi

# Create installer.xml from template file and place replace holders
ExecuteCommand "cp $PWD/pkg/installer/BitRock/installer.xml.in $PWD/pkg/installer/BitRock/installer.xml"
ReplacePlaceHolders $PWD/pkg/installer/BitRock/installer.xml

PackageInstaller "dotnet" "$PWD/inst" "DOTNET_INSTALLER_NAME_PREFIX" "$PWD/redux"

ExecuteCommand "rm -f $PWD/pkg/installer/BitRock/installer.xml"
