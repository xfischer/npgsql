#!/bin/bash

# ---------------------------
# This file contains functions that are used in other sh files on cm-dashboard
# ---------------------------

ShowGlobalVariables()
{
    MessageHeading "Global Variables & Values"

    Message $MSG_INFO "SCM_GIT_SERVER               $SCM_GIT_SERVER"
    Message $MSG_INFO "SCAN_BASE_DIR                $SCAN_BASE_DIR"
    Message $MSG_INFO "SCRIPTS_HOME                 $SCRIPTS_HOME"
    Message $MSG_INFO "PRODUCTS_HOME                $PRODUCTS_HOME"
    Message $MSG_INFO "LOGS_HOME_RELATIVE           $LOGS_HOME_RELATIVE"
    Message $MSG_INFO "LOGS_HOME                    $LOGS_HOME"
    Message $MSG_INFO "SOURCE_FOLDER_RELATIVE       $SOURCE_FOLDER_RELATIVE"
    Message $MSG_INFO "INSTALLATION_FOOTPRINT_FOLDER_RELATIVE     $INSTALLATION_FOOTPRINT_FOLDER_RELATIVE"
    Message $MSG_INFO "BINARIES_FOLDER_RELATIVE     $BINARIES_FOLDER_RELATIVE"
    Message $MSG_INFO "REMOTE_BUILDS                $REMOTE_BUILDS"
    Message $MSG_INFO "REMOTE_REDUX_STORE           $REMOTE_REDUX_STORE"
    Message $MSG_INFO "REDUX_STORE_DIR_BASE         $REDUX_STORE_DIR_BASE"
    Message $MSG_INFO "BUILDS_DIR_BASE              $BUILDS_DIR_BASE"
    Message $MSG_INFO "BUILDS_INSTALLER_BASE_DIR    $BUILDS_INSTALLER_BASE_DIR"
    Message $MSG_INFO "BUILDS_RPM_BASE_DIR          $BUILDS_RPM_BASE_DIR"
    Message $MSG_INFO "ERROR_LOG_SUMMARY            $ERROR_LOG_SUMMARY"
    Message $MSG_INFO "WARNING_LOG_SUMMARY          $WARNING_LOG_SUMMARY"
    Message $MSG_INFO "DAYS_TO_SEARCH_OLD_BUILDS    $DAYS_TO_SEARCH_OLD_BUILDS"
    Message $MSG_INFO "MSG_DEBUG                    $MSG_DEBUG"
    Message $MSG_INFO "MSG_EMPTY                    $MSG_EMPTY"
    Message $MSG_INFO "MSG_INFO                     $MSG_INFO"
    Message $MSG_INFO "MSG_WARN                     $MSG_WARN"
    Message $MSG_INFO "MSG_TEST                     $MSG_TEST"
    Message $MSG_INFO "MSG_TEST_VALUE               $MSG_TEST_VALUE"
    Message $MSG_INFO "MSG_CRITICAL                 $MSG_CRITICAL"
    Message $MSG_INFO "MSG_ERROR                    $MSG_ERROR"
    Message $MSG_INFO "MSG_HEADING                  $MSG_HEADING"
    Message $MSG_INFO "PRODUCTS                     ${PRODUCTS[*]}"
    Message $MSG_INFO "BLACKDUCK_DETECT_JAR         $BLACKDUCK_DETECT_JAR"
    Message $MSG_INFO "BLACKDUCK_DETECT_DOCKER_JAR  $BLACKDUCK_DETECT_DOCKER_JAR"
    Message $MSG_INFO "BLACKDUCK_URL                $BLACKDUCK_URL"
    Message $MSG_INFO "BLACKDUCK_API_TOKEN          $BLACKDUCK_API_TOKEN"
    Message $MSG_INFO "BLACKDUCK_DETECT_CLEANUP     $BLACKDUCK_DETECT_CLEANUP"
    Message $MSG_INFO "BLACKDUCK_TRUST_CERT         $BLACKDUCK_TRUST_CERT"
    Message $MSG_INFO "GENERATE_REPORTS             $GENERATE_REPORTS"
    Message $MSG_INFO "BLACKDUCK_DETECT_RISK_REPORT_PDF $BLACKDUCK_DETECT_RISK_REPORT_PDF"
    Message $MSG_INFO "BLACKDUCK_DETECT_RISK_REPORT_PDF_PATH    $BLACKDUCK_DETECT_RISK_REPORT_PDF_PATH"
    Message $MSG_INFO "BLACKDUCK_DETECT_NOTICES_REPORT  $BLACKDUCK_DETECT_NOTICES_REPORT"
    Message $MSG_INFO "BLACKDUCK_DETECT_NOTICES_REPORT_PATH $BLACKDUCK_DETECT_NOTICES_REPORT_PATH"
    Message $MSG_INFO "BLACKDUCK_DETECT_REPORT_TIMEOUT  $BLACKDUCK_DETECT_REPORT_TIMEOUT"
    Message $MSG_INFO "LOCATION                     $LOCATION"
}

# ---------------------------
# Message Helper Functions
# ---------------------------

# ---------------------------
# $1 - Heading text
MessageHeading()
{
    echo ""
    echo "-----------------------------------------------------"
    echo "$1"
    echo "-----------------------------------------------------"
}
# ---------------------------
# $1 - Message type
# $2 - Message text; in case of MSG_EMPTY, this value is ignored
Message()
{
    prefix="--> "
    MessageType="$1"
    MessageText="$2"
    DoExitOnError="$3"
    CacheOnWarning="$4"

    case $MessageType in
        "$MSG_DEBUG")
            if [[ -n "$VERBOSE" ]];
            then
                prefix="$prefix [Debug]"
                echo "$prefix $MessageText"
            fi
            ;;
        "$MSG_EMPTY")
            echo;
            ;;
        "$MSG_INFO")
            prefix="$prefix [Info]"
            echo "$prefix $MessageText"
            ;;
        "$MSG_WARN")
            prefix="$prefix [Warning]"
            echo "$prefix $MessageText"
            ;;
        "$MSG_TEST")
            MSG_IsTesting=1
            prefix="$prefix [Test]"
            echo -n "$prefix $MessageText... "
            ;;
        "$MSG_TEST_VALUE")
            echo "$2"
            ;;
        "$MSG_CRITICAL")
            if [ "$DoExitOnError" == true ];
            then
                prefixE="$prefix [Error]"

                MessageHeading "CRITICAL ERROR"
                touch $ERROR_LOG_SUMMARY

                echo "$prefixE Script exiting because of the following error:"
                echo
                echo "$prefix Scanning Product: $CURRENT_PRODUCT_SCANNED" | tee -a "$ERROR_LOG_SUMMARY"
                echo "$prefix Current working directory is $PWD" | tee -a "$ERROR_LOG_SUMMARY"
                dirs -l -v
                echo "$prefixE $MessageText" | tee -a "$ERROR_LOG_SUMMARY"
                echo "============================================================================" | tee -a "$ERROR_LOG_SUMMARY"

                echo
                echo
                exit 1
            elif [ "$CacheOnWarning" == true ];
            then
                prefixE="$prefix [Warning-Cache]"

                touch $WARNING_LOG_SUMMARY

                echo "$prefixE Failed in executing current command because of the following error:"
                echo "$prefix Product: $CURRENT_PRODUCT_SCANNED" | tee -a "$WARNING_LOG_SUMMARY"

                dirs -l -v
                echo "$prefixE $MessageText" | tee -a "$WARNING_LOG_SUMMARY"
                echo
            else
                prefixE="$prefix [Warning]"

                MessageHeading "WARNING"
                touch $WARNING_LOG_SUMMARY

                echo "$prefixE Failed in executing current command because of the following error:"
                echo
                echo "$prefix Product: $CURRENT_PRODUCT_SCANNED" | tee -a "$WARNING_LOG_SUMMARY"
                echo "$prefix Current working directory is $PWD" | tee -a "$WARNING_LOG_SUMMARY"
                dirs -l -v
                echo "$prefixE $MessageText" | tee -a "$WARNING_LOG_SUMMARY"
                echo "============================================================================" | tee -a "$WARNING_LOG_SUMMARY"

                echo
                echo
            fi
            ;;
        ?)
            exit 0
            ;;
    esac
}
#----------------------------

InitializeFolderStructure()
{
    MessageHeading "Creating Folder Structure"

    ExecuteCommand "mkdir -p $SCAN_BASE_DIR"
    ExecuteCommand "mkdir -p $SCRIPTS_HOME"
    ExecuteCommand "mkdir -p $PRODUCTS_HOME"
    ExecuteCommand "mkdir -p $LOGS_HOME"
}

# ---------------------------
CreateLogFile()
{
    MessageHeading "Create Log File for this run"
    CurrentTime=$(DateTimeStamp)
    Time=`date +%F_%H-%M-%S`

    ExecuteCommand "cd $LOGS_HOME"

    LOG_FILE="$LOGS_HOME/Scan-$Time.log"
    Message $MSG_INFO "Log File for current scan is $LOG_FILE"
    ExecuteCommand "touch $LOG_FILE"
}

# ---------------------------
# Shell/General Helper Functions
# ---------------------------

# ---------------------------
# A wrapper function for executing commands with printing debugging/info about
# the command and result to the log
# $1 - Command to be executed including any parameters/flags if any
ExecuteCommand()
{
    Message $MSG_DEBUG "[Function: $FUNCNAME] Executing Command: $@"

    eval "$@"
    commandStatus=$?

    Message $MSG_DEBUG "[Function: $FUNCNAME] Command returned status: $commandStatus"

    if [[ $commandStatus -ne 0 ]]; then
        Message $MSG_CRITICAL "[Function: $FUNCNAME] Failed to execute command - \"$@\"" true
    fi
}

# A wrapper function for executing commands with printing debugging/info about
# the command and result to the log. Incase of critical error it doesn't exit the job/command.
# $1 - Command to be executed including any parameters/flags if any
ExecuteCommandWithoutExit()
{
    Message $MSG_DEBUG "[Function: $FUNCNAME] Executing Command: $@"

    eval "$@"
    commandStatus=$?

    Message $MSG_DEBUG "[Function: $FUNCNAME] Command returned status: $commandStatus"

    if [[ $commandStatus -ne 0 ]]; then
        Message $MSG_CRITICAL "[Function: $FUNCNAME] Failed to execute command - \"$@\"" false
    fi
}
# A wrapper function for executing commands with printing debugging/info about
# the command and result to the log. Incase of critical error it doesn't exit the job/command.
# $1 - Command to be executed including any parameters/flags if any
ExecuteCommandWithoutExitAndCacheWarnings()
{
    Message $MSG_DEBUG "[Function: $FUNCNAME] Executing Command: $@"

    eval "$@"
    commandStatus=$?

    Message $MSG_DEBUG "[Function: $FUNCNAME] Command returned status: $commandStatus"

    if [[ $commandStatus -ne 0 ]]; then
        Message $MSG_CRITICAL "[Function: $FUNCNAME] Failed to execute command - \"$@\"" false true
    fi
}

# Checks whether given function exists or not in the sourced script files
# $1 - Function Name
IsValidFunction()
{
    type $1 > /dev/null 2>&1

    if [[ $? -eq 0 ]];
    then
        echo true
    else
        echo false
    fi
}

# ---------------------------
# A wrapper function for executing functions with printing debugging/info about
# the function to the log
# $1 - Function Name
RunLocalFunction()
{
    FunctionName="$1"
    ShouldContinue=true

    if [[ $(IsValidFunction "${FunctionName}_local") == true ]];
    then
        ShouldContinue=$(${FunctionName}_local ${*:2})
    fi

    echo $ShouldContinue
}

# ---------------------------
TestVariableNonEmpty()
{
    VariableName="$1"

    if [[ -z "${!VariableName}" ]];
    then
        Message $MSG_CRITICAL "Variable [$VariableName] is null or empty."
    else
        Message $MSG_INFO "Variable [$VariableName] = ${!VariableName}"
    fi
}

# ---------------------------
PushD()
{
    strDir="$1"

    ExecuteCommand "pushd $strDir > /dev/null"
    Message $MSG_INFO "Entering folder $strDir"
}

# ---------------------------
PopD()
{
    strDir=$PWD
    ExecuteCommand "popd > /dev/null"
    Message $MSG_INFO "Leaving folder $strDir"
}

# ---------------------------
Control_C()
{
    MessageHeading "Got Ctrl+C"
    Message $MSG_INFO "Exiting... Bye!"
    exit 0
}

DateTimeStamp()
{
    echo `date +%F_%H-%M-%S_000000`
}

# Trapping Interrupts
if [[ -z "$SIGINT_IGNORE" ]];
then
    trap Control_C SIGINT
fi

CheckDirExists()
{
    if [ -d $1 ]; then
        return 0
    else
        Message $MSG_CRITICAL "Dir $1 does not exits"
        return 1
    fi
}

CheckFileExists()
{
    if [ -d $1 ]; then
        return 0
    else
        Message $MSG_CRITICAL "File $1 does not exits"
        return 1
    fi
}

# This function moves all files from source to target directory,
# and deletes all from source directory
# Arg $1 Source DIR
# Arg $2 TargetDIR

MoveFiles()
{
    SourceDir=$1
    TargetDir=$2

    if [[ "$SourceDir" = "/" || "$TargetDir" = "/" ]]; then
        Message $MSG_CRITICAL "Source and Target DIR can't be \"/\" "
    fi

    MessageHeading "Moving files from $SourceDir to $TargetDir"

    if CheckDirExists $SourceDir && CheckDirExists $TargetDir; then
        Message $MSG_INFO "Source Dir is $SourceDir"
        Message $MSG_INFO "Target DIR is $TargetDir"

        ExecuteCommand  "cp -rp  $SourceDir/* $TargetDir/ "
    fi
}

SetPermissions()
{
        MessageHeading "SetPermissions() Change File Permissions of file in $PWD"

        if [ "$SourceDir" = "/"  ]; then
                Message $MSG_CRITICAL "You are trying to change permission of '/', Not Allowed"
                break
        fi

        if [ "$(ls -A $PWD)" ]; then

                Message $MSG_INFO "Changing File Permissions"

                ExecuteCommand "find . $1 | grep -v \"^\.\" | xargs file | grep    directory | sed \"s:\:::g\" | awk '{print \"chmod -v -R 755 \" \$1}' "
                ExecuteCommand "find . $1 | grep -v \"^\.\" | xargs file | grep -v directory | sed \"s:\:::g\" | awk '{print \"chmod -v    644 \" \$1}' "
        else
           Message $MSG_INFO "Current Directory is empty"
        fi
}

CleanFolder()
{
    Dir=$1

    if [ ! -d "$Dir" ]; then
        Message $MSG_WARN "$Dir does not exist"
    fi

    MessageHeading "Cleaning content of folder $Dir"

    if [ "$Dir" = "/"  ]; then
        Message $MSG_CRITICAL "You are trying to delete everything in '/', Not Allowed!"
        break
    fi

    if [[ "$Dir" = "$SCAN_BASE_DIR" ]] || [[ "$Dir" = "$SCRIPTS_HOME" ]] || [[ "$Dir" = "$PRODUCTS_HOME" ]] || [[ "$Dir" = "$LOGS_HOME" ]]; then
        Message $MSG_INFO "$Dir is not allowed to be emptied"
    else
        ExecuteCommand "rm -fr $Dir/*"
    fi
}

KillDuplicate()
{
    if [[ $(ps -ef | grep wrapper.sh | wc -l) -gt 1 ]];
    then
        Message $MSG_INFO "Wrapper (scanning) process is already running so kill it first."
        #ps -ef | grep wrapper.sh | awk '{print "kill -9 $2"}' | sh
        kill $(ps aux | grep 'wrapper.sh' | awk '{print $2}')
    fi
}

CleanLogs()
{
    MessageHeading "Clean Up Logs"

    Message $MSG_INFO "Cleaning $LOG_FOLDER. Deleting logs older than 30 days"
    ExecuteCommand "cd $LOG_FOLDER"
    find $LOG_FOLDER -type f -name '*.log' -mtime +30 -exec rm {} \;

    RSYNC_LOG_FILE="/var/log/rsync.log"
    if [ ! -f $RSYNC_LOG_FILE ];
    then
        return
    fi

    RSYNC_LOG_SIZE=`du -sm $RSYNC_LOG_FILE | cut -f -1`
    Message $MSG_INFO "Remove $RSYNC_LOG_FILE file it is more than 10MB in size."
    if [[ $RSYNC_LOG_SIZE -gt 10 ]];
    then
        ExecuteCommand "rm -f $RSYNC_LOG_FILE"
        ExecuteCommand "touch $RSYNC_LOG_FILE"
    fi
}

Scan()
{
    Product_Scan_Options="$@"

    MessageHeading "Scanning $PRODUCT-$VERSION"
    ExecuteCommand "cd $PRODUCT_FOLDER"

    Message $MSG_INFO "$PRODUCT-$VERSION specific scan parameters are: $Product_Scan_Options"

    scan_location_name=""

    if [[ -n "$PLATFORM_TO_SCAN" ]];
    then
        if [[ -n "$PPAS_VERSION" ]];
        then
            scan_location_name="$PRODUCT-$VERSION-$PLATFORM_TO_SCAN-as$PPAS_VERSION-Signature"
        else
            scan_location_name="$PRODUCT-$VERSION-$PLATFORM_TO_SCAN-Signature"
        fi
    else
        scan_location_name="$PRODUCT-$VERSION-Signature"
    fi

    ExecuteCommand "java -jar $BLACKDUCK_DETECT_JAR --blackduck.url=$BLACKDUCK_URL \
--blackduck.api.token=$BLACKDUCK_API_TOKEN \
--detect.project.name=$PRODUCT \
--detect.project.version.name=$VERSION \
-–detect.cleanup=$BLACKDUCK_DETECT_CLEANUP \
--detect.tools=DETECTOR,SIGNATURE_SCAN,IMPACT_ANALYSIS \
--blackduck.trust.cert=$BLACKDUCK_TRUST_CERT \
--detect.parallel.processors=$BLACKDUCK_PARALLEL_PROCESSOR \
--detect.code.location.name=$scan_location_name \
--detect.blackduck.signature.scanner.license.search=true \
--detect.detector.search.depth=$BLACKDUCK_DETECT_SEARCH_DEPTH \
--detect.excluded.detector.types=$BLACKDUCK_DETECT_EXCLUDED_DETECTOR_TYPES \
--detect.bom.aggregate.name=$PRODUCT-$VERSION-BOM \
$Product_Scan_Options "

}

Scan_Binaries()
{
    Product_Binaries_Folder=$1

    MessageHeading "Scanning Binaries $PRODUCT-$VERSION"
    Message $MSG_INFO "$PRODUCT-$VERSION binaries folder is: $Product_Binaries_Folder"
    ExecuteCommand "cd $PRODUCT_FOLDER"

    if [ -d "$Product_Binaries_Folder" ];
    then
        ExecuteCommand "zip -r $PRODUCT-$VERSION-binaries.zip $Product_Binaries_Folder"
    else
        Message $MSG_WARN "No such folder $Product_Binaries_Folder to zip and scan"
        return
    fi

    scan_location_name=""

    if [[ -n "$PLATFORM_TO_SCAN" ]];
    then
        if [[ -n "$PPAS_VERSION" ]];
        then
            scan_location_name="$PRODUCT-$VERSION-$PLATFORM_TO_SCAN-as$PPAS_VERSION-Binary"
        else
            scan_location_name="$PRODUCT-$VERSION-$PLATFORM_TO_SCAN-Binary"
        fi
    else
        scan_location_name="$PRODUCT-$VERSION-Binary"
    fi

    if [ -f "$PRODUCT_FOLDER/$PRODUCT-$VERSION-binaries.zip" ];
    then
        ExecuteCommand "java -jar $BLACKDUCK_DETECT_JAR --blackduck.url=$BLACKDUCK_URL \
--blackduck.api.token=$BLACKDUCK_API_TOKEN \
--detect.project.name=$PRODUCT \
--detect.project.version.name=$VERSION \
-–detect.cleanup=$BLACKDUCK_DETECT_CLEANUP \
--detect.tools=BINARY_SCAN \
--blackduck.trust.cert=$BLACKDUCK_TRUST_CERT \
--detect.parallel.processors=$BLACKDUCK_PARALLEL_PROCESSOR \
--detect.code.location.name=$scan_location_name \
--detect.detector.search.depth=$BLACKDUCK_DETECT_SEARCH_DEPTH \
--detect.clone.project.version.latest=true \
--detect.binary.scan.file.path=$PRODUCT_FOLDER/$PRODUCT-$VERSION-binaries.zip"
    else
        Message $MSG_WARN "No such file $PRODUCT_FOLDER/$PRODUCT-$VERSION-binaries.zip for Binary scan"
        return
    fi
}


Scan_Containers()
{
    Container_Name=$1
    Product_Scan_Options=$2

    MessageHeading "Scanning $PRODUCT-$VERSION"
    ExecuteCommand "cd $PRODUCT_FOLDER"

    Message $MSG_INFO "$PRODUCT-$VERSION specific scan parameters are: $Product_Scan_Options"

    ExecuteCommand "java -jar $BLACKDUCK_DETECT_JAR --blackduck.url=$BLACKDUCK_URL \
--blackduck.api.token=$BLACKDUCK_API_TOKEN \
--detect.project.name=$PRODUCT \
--detect.project.version.name=$VERSION \
-–detect.cleanup=$BLACKDUCK_DETECT_CLEANUP \
--blackduck.trust.cert=$BLACKDUCK_TRUST_CERT \
--detect.tools=SIGNATURE_SCAN,DOCKER \
--detect.parallel.processors=$BLACKDUCK_PARALLEL_PROCESSOR \
--detect.code.location.name=$PRODUCT-$VERSION-Container-2-as12 \
--detect.detector.search.depth=$BLACKDUCK_DETECT_SEARCH_DEPTH \
--detect.docker.inspector.path=$BLACKDUCK_DETECT_DOCKER_JAR \
--detect.docker.image=localhost:5000/$Container_Name \
--detect.docker.platform.top.layer.id=sha256:c3cef396d4cad3de637522f00b032178c0a9e362cf390920cf42b427605cc672 \
--logging.level.detect=DEBUG \
$Product_Scan_Options "

}

Generate_BD_Default_Reports()
{
    Report_Generation_Configuration=""

    MessageHeading "Generating Reports for: $PRODUCT-$VERSION"
    ExecuteCommand "cd $PRODUCT_FOLDER"

    ExecuteCommand "java -jar $BLACKDUCK_DETECT_JAR --blackduck.url=$BLACKDUCK_URL \
--blackduck.api.token=$BLACKDUCK_API_TOKEN \
--detect.project.name=$PRODUCT \
--detect.project.version.name=$VERSION \
-–detect.cleanup=$BLACKDUCK_DETECT_CLEANUP \
--blackduck.trust.cert=$BLACKDUCK_TRUST_CERT \
--detect.tools=NONE \
--detect.excluded.detector.types=ALL \
--detect.parallel.processors=$BLACKDUCK_PARALLEL_PROCESSOR \
--detect.risk.report.pdf=$BLACKDUCK_DETECT_RISK_REPORT_PDF \
--detect.notices.report=true --detect.notices.report.path=$BLACKDUCK_DETECT_NOTICES_REPORT_PATH \
--detect.risk.report.pdf.path=$BLACKDUCK_DETECT_RISK_REPORT_PDF_PATH --detect.report.timeout=$BLACKDUCK_DETECT_REPORT_TIMEOUT"
}

RSync_Files()
{
    SOURCE_FILE_PATH=$1
    DESTINATION_PATH=$2

    Message $MSG_INFO "Rsync(ing) remote $SOURCE_FILE_PATH to local $DESTINATION_PATH "

    ExecuteCommandWithoutExit  "$RSYNC_COMMAND $SOURCE_FILE_PATH $DESTINATION_PATH"
}

ExtractRPMs()
{
    TargetDir=$1

    MessageHeading "Extracting All RPMs in $TargetDir to same directory"
    ExecuteCommand "cd $TargetDir"

    Message $MSG_INFO "Removing src, debug, cto rpm, as we don't need to scan src rpm"
    ExecuteCommand "rm -f *src*.rpm"
    ExecuteCommand "rm -f *debug*.rpm"
    ExecuteCommand "rm -f *cto*.rpm"

    Message $MSG_INFO "Removing regression logs and logs dirs"
    ExecuteCommand "rm -fr *logs*"
    ExecuteCommand "rm -fr *.log"

    Message $MSG_INFO "Removing *full.tar*"
    ExecuteCommand "rm -fr *tar.bz2"
    ExecuteCommand "rm -fr *tar.gz"

    Message $MSG_INFO "Starting RPMs extraction in $TargetDir"
    ExecuteCommand "find * -type f | grep rpm$ | xargs -I{} echo \"echo {} ; rpm2cpio {} | cpio -idmv\" | sh"
    ExecuteCommand "cd $TargetDir"
    ExecuteCommand "rm -f *.rpm"
}

ExtractDebianPackages()
{
    TargetDir=$1

    MessageHeading "Extracting All debian packages in $TargetDir to same directory"

    ExecuteCommand "cd $TargetDir"
    Message $MSG_INFO "Starting DEBs extraction in $TargetDir"
    ExecuteCommand "find * -type f | grep deb$ | xargs -I{} echo \"echo {} ; dpkg-deb -xv {} $TargetDir\" | sh"
    ExecuteCommand "cd $TargetDir"
    ExecuteCommand "rm -f *.deb"
    ExecuteCommand "rm -f *.ddeb"
}

ExtractWindowsStaging()
{
    TargetDir=$1

    MessageHeading "Extracting windows staging(tar.gz, zip) present in $TargetDir to same directory"

    Message $MSG_INFO "Starting Linux/Windows Installer staging extraction in $TargetDir"
    ExecuteCommand "cd $TargetDir"

    ExecuteCommandWithoutExit "find * -type f ! -path */pgadmin4/* | grep zip$ | xargs -I{} echo \" echo {}; unzip {} \" | sh"
    ExecuteCommandWithoutExit "find * -type f ! -path */pgadmin4/* | grep tar.gz$ | xargs -I{} echo \" echo {}; tar -xvf {} \" | sh"

    #Delete Staging dir
    ExecuteCommand "rm -fr StagingBinaries"

    #Delete window.tar.gz file that is present in 9.6 and prior versions
    ExecuteCommand "rm -fr window*tar.gz"
    ExecuteCommand "rm -fr Window*tar.gz"
    ExecuteCommandWithoutExit "find * -type f | grep zip$ | xargs -I{} echo \" echo {}; rm -f {} \" | sh"

    # ExecuteCommandWithoutExit "[[ -d c/edbas/builds.edb/inst/logs ]] && rm -fr c/edbas/builds.edb/inst/logs"
    # ExecuteCommandWithoutExit "[[ -d c/edbas/builds.edb/inst/regression ]] && rm -fr c/edbas/builds.edb/inst/regression"
    # ExecuteCommandWithoutExit "[[ -d c/edbas/builds.edb/inst/src ]] && rm -fr c/edbas/builds.edb/inst/src"

    ExecuteCommandWithoutExit "rm -fr *.zip"
    #ExecuteCommandWithoutExit "rm -fr *.exe"
    ExecuteCommandWithoutExit "rm -fr *log*"
    ExecuteCommandWithoutExit "rm -fr lib_list_files"
    ExecuteCommandWithoutExit "rm -fr DebugSymbols"
}

ExtractLinuxStaging()
{
    TargetDir=$1

    MessageHeading "Extracting linux staging(tar.gz) present in $TargetDir to same directory"

    Message $MSG_INFO "Starting Linux/Windows Installer staging extraction in $TargetDir"

    ExecuteCommand "cd $TargetDir"
    ExecuteCommand "find * -type f | grep tar.gz$ | xargs -I{} echo \"  echo {}; tar -xvf {} \" | sh"

    ExecuteCommand "cd $TargetDir"
    ExecuteCommand "find * -type f | grep tar.gz$ | xargs -I{} echo \" echo {}; rm -f {} \" | sh"
}

CheckoutBranch()
{
    RepoDir=$1
    Branch=$2
    # ExecuteCommandWithoutExit "git reset --hard"
    # ExecuteCommandWithoutExit "git clean -f"
    ExecuteCommandWithoutExit "git --git-dir=$RepoDir/.git --work-tree=$RepoDir checkout $Branch"
}

CloneSource()
{
    RepoName=$1
    Branch=$2
    LocalPath=$3

    ExecuteCommandWithoutExit "git clone $SCM_GIT_SERVER/$RepoName $LocalPath/$RepoName"
    echo ""
    CheckoutBranch "$LocalPath/$RepoName" "$Branch"
    echo ""
}

DeleteUnWantedFootprint()
{
    ExecuteCommand "rm -f *.zip"
    ExecuteCommand "rm -f *tar.bz2"
   #ExecuteCommand "rm -f *.exe"
    ExecuteCommand "rm -f *.log"
    ExecuteCommand "rm -fr logs"
    ExecuteCommand "rm -fr lib_list_files"
    ExecuteCommand "rm -fr regression_logs"
}

ExtractPackages()
{
    Installation_Footprint_Dir=$1

    ExecuteCommand "cd $Installation_Footprint_Dir"
    Message $MSG_INFO "PWD: `pwd`"

    for f in *;
    do
        ExecuteCommand "cd $Installation_Footprint_Dir"

        if [ -d "$f" ];
        then

            MessageHeading "Extracting packages for $PRODUCT-$VERSION in $Installation_Footprint_Dir"
            ExecuteCommand "cd $f"
            Message $MSG_INFO "PWD: `pwd`"
            #DeleteUnWantedFootprint

            if [[ "$f" == *"centos"* || "$f" == *"rhel"* ]];
            then
                ExtractRPMs "$Installation_Footprint_Dir/$f"
            elif [[ "$f" == *"deb"* ]];
            then
                ExtractDebianPackages "$Installation_Footprint_Dir/$f"
            elif [[ "$f" == *"suse"* || "$f" == *"sles"* ]];
            then
                ExtractRPMs "$Installation_Footprint_Dir/$f"
            elif [[ "$f" == *"ubuntu"* ]];
            then
                ExtractDebianPackages "$Installation_Footprint_Dir/$f"
            elif [[ "$f" == *"ppc"* ]];
            then
                ExtractRPMs "$Installation_Footprint_Dir/$f"
            elif [[ "$f" == *"windows"* ]];
            then
                ExtractWindowsStaging "$Installation_Footprint_Dir/$f"
            elif [[ "$f" == *"linux"* ]];
            then
                ExtractLinuxStaging "$Installation_Footprint_Dir/$f"
            fi

            DeleteUnWantedFootprint
        fi
    done
}

CopyBinariesToBinariesFolder()
{
    Installation_Footprint_Dir=$1
    Binaries_Dir=$2

    MessageHeading "Copying binaries from $Installation_Footprint_Dir to $Binaries_Dir"

    ExecuteCommand "cd $Installation_Footprint_Dir"
    Message $MSG_INFO "PWD: `pwd`"

    for f in *;
    do
        ExecuteCommand "cd $Installation_Footprint_Dir"

        if [ -d "$f" ]; then

            Message $MSG_INFO "Source DIR: $Installation_Footprint_Dir/$f"
            Message $MSG_INFO "Target DIR: $Binaries_Dir/$f"

            ExecuteCommand "mkdir -p $Binaries_Dir/$f"
            ExecuteCommand "cd $f"
            ExecuteCommand "find . -executable -type f -not -type d | xargs -I{} echo \" echo '{}'; cp '{}' $Binaries_Dir/$f/;\" | sh"
            ExecuteCommand "find . -type f -name '*.jar' | xargs -I{} echo \" echo '{}'; cp '{}' $Binaries_Dir/$f/;\" | sh"
            ExecuteCommand "rm -f $Binaries_Dir/$f/*.sql"
            ExecuteCommand "rm -f $Binaries_Dir/$f/*.txt"
            ExecuteCommand "rm -f $Binaries_Dir/$f/*.html"
            ExecuteCommand "rm -f $Binaries_Dir/$f/*.doc"
            ExecuteCommand "rm -f $Binaries_Dir/$f/*.pdf"
            ExecuteCommand "rm -f $Binaries_Dir/$f/*.log"
            ExecuteCommand "rm -f $Binaries_Dir/$f/*edb-postgres*"
            ExecuteCommand "rm -f $Binaries_Dir/$f/*libpq*"
            ExecuteCommand "rm -f $Binaries_Dir/$f/*.pyd*"
            ExecuteCommand "rm -f $Binaries_Dir/$f/*python37.dll*"
            ExecuteCommand "rm -f $Binaries_Dir/$f/*Zlib.dll*"
            ExecuteCommand "rm -f $Binaries_Dir/$f/alembic.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/chardetect.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/easy_install-3.8.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/easy_install.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/easy_install3.8.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/easy_install3.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/email_validator.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/flask.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/mako-render.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/pip-3.8.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/pip.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/pip3.8.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/pip3.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/pybabel.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/pygmentize.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/sphinx-apidoc.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/sphinx-autogen.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/sphinx-build.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/sphinx-quickstart.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/sqlformat.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/sshtunnel.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/wheel-3.8.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/wheel.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/wheel3.8.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/wheel3.exe"
            ExecuteCommand "rm -f $Binaries_Dir/$f/libsnmp++.a"
            ExecuteCommand "rm -f $Binaries_Dir/$f/vcredist*"
        fi
    done
}

DownloadTar()
{
    Tar_File=$1

    ExecuteCommandWithoutExit "wget http://redux-store.ox.uk.enterprisedb.com/store/PackageSource/$Tar_File"
    ExecuteCommandWithoutExit "tar -xvf $Tar_File"
    ExecuteCommandWithoutExit "rm -f $Tar_File"
}

CreateAndActivatePythonVirtualEnv()
{
    ExecuteCommand "cd $SOURCE_FOLDER"
    ExecuteCommand "python3 -m venv env"
    ExecuteCommand "source env/bin/activate"
    ExecuteCommand "which python"
    ExecuteCommand "python -V"
    ExecuteCommand "sudo pip3 install --upgrade pip"
}

DeactivatePythonVirtualEnv()
{
    ExecuteCommand "deactivate"
}

InstallPythonRequirements()
{
    Requirements_File_Path="$1"
    ExecuteCommand "pip install --upgrade pip"
    ExecuteCommand "sudo pip3 install setuptools_rust"
    ExecuteCommand "pip3 install psycopg2-binary"
    ExecuteCommand "pip3 install -r $Requirements_File_Path"
}

ClearDirs()
{
    MessageHeading "$PRODUCT-$VERSION: Delete old Source & installation footprint folders"

    ExecuteCommand "rm -fr $SOURCE_FOLDER"
    ExecuteCommand "rm -fr $INSTALLATION_FOOTPRINT_FOLDER"
    ExecuteCommand "rm -fr $BINARIES_FOLDER"
    ExecuteCommand "rm -fr $PRODUCT_FOLDER/*.zip"
    ExecuteCommand "rm -fr $PRODUCT_FOLDER/*binaries*"
}

CreateDirs()
{
    MessageHeading "$PRODUCT-$VERSION: Create Source & installation footprint folders"

    ExecuteCommand "mkdir -p $SOURCE_FOLDER"
    ExecuteCommand "mkdir -p $INSTALLATION_FOOTPRINT_FOLDER"
    ExecuteCommand "mkdir -p $BINARIES_FOLDER"

    for platform in ${PLATFORM_LIST};
    do
        mkdir -p $INSTALLATION_FOOTPRINT_FOLDER/$platform
    done

}

UploadReportsToBuilds()
{
    MessageHeading "Upload reports to builds.enterprisedb.com"

    CurrentDate=`date +%F`

    ExecuteCommand "ssh $REMOTE_BUILDS \"cd $REMOTE_REPORTS_FOLDER; mkdir -p $CurrentDate;\" "
    ExecuteCommand "cd $REPORTS_HOME"
    ExecuteCommand "scp * $REMOTE_BUILDS:$REMOTE_REPORTS_FOLDER/$CurrentDate"
}

# This function is used for old directory structure on builds.enterprisedb.com (/mnt/DailyBuilds/*****)
# Older directory structure was not in uniform structure so there were a lot conditionals or format in
# directory names

PullPackagesFromBuilds()
{

    BuildsPathPrefix=$1
    Folder_Version=$2
    declare -a Locations=("$3")
    PackagetoLookFor=$4
    LocalDestinationFolder=$5
    BuildsPathPostfix=$6

    MessageHeading "Start: Get $PackagetoLookFor of $PRODUCT-$VERSION from builds.enterprisedb.com, that was built in last $DAYS_TO_SEARCH_OLD_BUILDS days."

    # If builds Path has BuildsPathPostfix provided as an argument, then append / (slash) to it at end.
    if [ ! -z "$BuildsPathPostfix" ]
    then
        BuildsPathPostfix="$BuildsPathPostfix/"
    fi

    Counter=$DAYS_TO_SEARCH_OLD_BUILDS
    while [[ $Counter -gt -1 ]];
    do
        CurrentDate=`date +%Y-%m-%d -d "$Counter day ago"`

        for location in $Locations
        do
            ExecuteCommandWithoutExitAndCacheWarnings "scp buildfarm@$BuildsPathPrefix/$CurrentDate/$Folder_Version/$location/${BuildsPathPostfix}${PackagetoLookFor} $LocalDestinationFolder"
        done

        ((Counter--))
    done

    MessageHeading "End: Get $PackagetoLookFor of $PRODUCT-$VERSION from builds.enterprisedb.com, that was built in last $DAYS_TO_SEARCH_OLD_BUILDS days."
    echo
}


# This function is used for new directory structure on builds.enterprisedb.com (/mnt/daily_builds/*****)
# New structure is uniform and consistent in structure for different products and daily builds.
# /mnt/builds/daily-builds/$LOCATION/edb_OR_pg/****
CopyPackagesFromBuilds()
{

    BuildsPathPrefix=$1
    Folder_Version=$2
    declare -a Locations=("$3")
    PackagetoLookFor=$4
    LocalDestinationFolder=$5
    BuildsPathPostfix=$6

    MessageHeading "Start: Get $PackagetoLookFor of $PRODUCT-$VERSION from builds.enterprisedb.com, that was built in last $DAYS_TO_SEARCH_OLD_BUILDS days."

    # If builds Path has BuildsPathPostfix provided as an argument, then append / (slash) to it at end.
    if [ ! -z "$BuildsPathPostfix" ]
    then
        BuildsPathPostfix="$BuildsPathPostfix/"
    fi

    Counter=$DAYS_TO_SEARCH_OLD_BUILDS
    while [[ $Counter -gt -1 ]];
    do
        CurrentDate=`date +%Y-%m-%d -d "$Counter day ago"`

        for location in $Locations
        do
            ExecuteCommandWithoutExitAndCacheWarnings "scp buildfarm@$BuildsPathPrefix/$CurrentDate/$Folder_Version/$location/${BuildsPathPostfix}${PackagetoLookFor} $LocalDestinationFolder"
        done

        ((Counter--))
    done

    MessageHeading "End: Get $PackagetoLookFor of $PRODUCT-$VERSION from builds.enterprisedb.com, that was built in last $DAYS_TO_SEARCH_OLD_BUILDS days."
    echo
}

CloneSourceFromGitHub()
{
    RepoName=$1
    Branch=$2
    LocalPath=$3
    PublicGitHubRepo=$4

    GITHUB_REPO="$SCM_GITHUB_SERVER/$RepoName"

    if [ ! -z "$PublicGitHubRepo" ]
    then
        GITHUB_REPO="$PublicGitHubRepo"
    fi

    ExecuteCommandWithoutExit "git clone $GITHUB_REPO $LocalPath/$RepoName"
    echo ""
    CheckoutBranch "$LocalPath/$RepoName" "$Branch"
    echo ""
}

GetPackageURL()
{
  if [ -n "$PACKAGE_VERSION" ]; then
    VER="${PACKAGE_VERSION}"
  fi
  if [ -n "$ARCH" ]; then
    ARCH="architecture:$ARCH}"
  fi

  url=$(cloudsmith list packages -q "name:${package} AND ${VER}" -l 200 ${CLOUDSMITH_ORGANIZATION}/${CLOUDSMITH_REPO_NAME} -F json | jq -r '.data[].cdn_url')
  # Replace basic with cloudsmith token
  url=`echo $url | sed "s|basic|$CLOUDSMITH_TOKEN|g"`
  echo $url
}

DownloadPackage()
{
  wget --http-user=${CLOUDSMITH_ORGANIZATION} --http-password=${CLOUDSMITH_TOKEN} $url
}

DownloadWindowsStagingBinaries()
{
        BUILDS_SERVER_PATH="$1"
        DEST_DIR="$2"

        echo "Downloading $REMOTE_BUILDS:$BUILDS_SERVER_PATH inside $DEST_DIR"
        wget --user=$BUILDS_SERVER_USER --password=$BUILDS_SERVER_PASSWORD $BUILDS_SERVER_PATH
        mv windows.zip $DEST_DIR

}

InstallBuildRequirements()
{
  sudo yum install -y krb5-devel gcc
}