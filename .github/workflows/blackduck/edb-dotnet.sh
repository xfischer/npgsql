#!/bin/bash 

export PRODUCT="EDB-DotNet"
export VERSION=$(echo ${PACKAGE_VERSION} | cut -f1 -d-)
export REMOTE_PRODUCT_REDUX_DIR="$REDUX_STORE_DIR_BASE/dotnet/latest/*"

# Checkout Tags/Branches
#------------------------

export REPO_DOTNET='edb-dotnet'
export BRANCH_DOTNET='master'

export PRODUCT_FOLDER=$PRODUCTS_HOME/$PRODUCT-$VERSION
export SOURCE_FOLDER=$PRODUCT_FOLDER/$SOURCE_FOLDER_RELATIVE
export INSTALLATION_FOOTPRINT_FOLDER=$PRODUCT_FOLDER/$INSTALLATION_FOOTPRINT_FOLDER_RELATIVE
export BINARIES_FOLDER=$PRODUCT_FOLDER/$BINARIES_FOLDER_RELATIVE

ShowLocalVariables()
{
    MessageHeading "$PRODUCT-$VERSION: Variables & Values"
    
    Message $MSG_INFO "PRODUCT_FOLDER           $PRODUCT_FOLDER"
    Message $MSG_INFO "SOURCE_FOLDER            $SOURCE_FOLDER"
    Message $MSG_INFO "INSTALLATION_FOOTPRINT   $INSTALLATION_FOOTPRINT_FOLDER"
    Message $MSG_INFO "BINARIES_FOLDER          $BINARIES_FOLDER"
    Message $MSG_INFO "REPO_DOTNET              $REPO_DOTNET"
    Message $MSG_INFO "BRANCH_DOTNET            $BRANCH_DOTNET"
}

CopySources()
{
    MessageHeading "$PRODUCT-$VERSION: Copy sources in $SOURCE_FOLDER"
    mkdir -p $SOURCE_FOLDER
    cp -rp ${GITHUB_WORKSPACE}/* $SOURCE_FOLDER 
}

CopyPackages()
{
    MessageHeading "Copying packages for $PRODUCT-$VERSION in $INSTALLATION_FOOTPRINT_FOLDER"

    cd /tmp
    for package in ${PACKAGES};
    do
      url=$(GetPackageURL $package)

      # Check if url is not empty
      if [ -z "$url" ]; then
        echo "Package not found."
      else
        DownloadPackage $url
      fi
    done
    rm -f *.rpm.* *.deb.* || true
    cd -

    # Download Windows binaries
    if [ -n "$WINDOWS_X64_STAGING_BINARIES_PATH" ]; then
       DownloadWindowsStagingBinaries $WINDOWS_X64_STAGING_BINARIES_PATH $INSTALLATION_FOOTPRINT_FOLDER/windows-x64
    fi

    ls -lrt /tmp

    # cp /tmp/*rhel7*x86_64* $INSTALLATION_FOOTPRINT_FOLDER/rhel7-x86_64
    # cp /tmp/*rhel8*noarch* $INSTALLATION_FOOTPRINT_FOLDER/rhel8-x86_64
    # cp /tmp/*rhel8*noarch* $INSTALLATION_FOOTPRINT_FOLDER/rhel8-ppc64le
    # cp /tmp/*rhel8*noarch* $INSTALLATION_FOOTPRINT_FOLDER/rhel8-s390x

    # cp /tmp/*sles12*noarch* $INSTALLATION_FOOTPRINT_FOLDER/sles12-x86_64
    # cp /tmp/*sles12*noarch* $INSTALLATION_FOOTPRINT_FOLDER/sles12-ppc64le
    # cp /tmp/*sles12*noarch* $INSTALLATION_FOOTPRINT_FOLDER/sles12-s390x

    # cp /tmp/*sles15*noarch* $INSTALLATION_FOOTPRINT_FOLDER/sles15-x86_64
    # cp /tmp/*sles15*noarch* $INSTALLATION_FOOTPRINT_FOLDER/sles15-ppc64le
    # cp /tmp/*sles15*noarch* $INSTALLATION_FOOTPRINT_FOLDER/sles15-s390x

    # cp /tmp/*ubuntu4* $INSTALLATION_FOOTPRINT_FOLDER/ubuntu18-amd64
    # cp /tmp/*ubuntu5* $INSTALLATION_FOOTPRINT_FOLDER/ubuntu20-amd64
    # cp /tmp/*deb9* $INSTALLATION_FOOTPRINT_FOLDER/debian9-amd64
    # cp /tmp/*deb10* $INSTALLATION_FOOTPRINT_FOLDER/debian10-amd64

    ExecuteCommand "mkdir -p $INSTALLATION_FOOTPRINT_FOLDER/ascommon"
}

export BLACKDUCK_DETECT_EXCLUDED_DETECTOR_TYPES="cpan,nuget,git,pip,maven"

if $GENERATE_REPORTS;
then
    Generate_BD_Default_Reports
    return $?
fi

MessageHeading "Start $PRODUCT-$VERSION at: `date +%F_%H-%M-%S`"
StartTime=`date +%s`

ShowLocalVariables
ClearDirs
CreateDirs
CopySources
CopyPackages
ExtractPackages "$INSTALLATION_FOOTPRINT_FOLDER"
Scan

CopyBinariesToBinariesFolder "$INSTALLATION_FOOTPRINT_FOLDER" "$BINARIES_FOLDER"
Scan_Binaries "$BINARIES_FOLDER"

EndTime=`date +%s`
TimeTaken=`expr $EndTime - $StartTime`
ELAPSED="$(($TimeTaken / 3600))hrs $((($TimeTaken / 60) % 60))min $(($TimeTaken % 60))sec"

Message "End of Scan $PRODUCT-$VERSION at: `date +%F_%H-%M-%S`"
MessageHeading "End $PRODUCT-$VERSION: Time Taken $ELAPSED"
