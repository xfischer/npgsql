#!/bin/bash

source settings.sh
source helperfunctions.sh

# Save full path to wrapper
export WD="`pwd`/$(dirname $0)/"
VERBOSE=1

usage()
{
    cat << EOF
$(MessageHeading "Usage")
usage: $0 OPTIONS

OPTIONS can be:
    -h      Show this message
    -g      Generate BlackDuck default reports (reports are for EDB INTERNAL USE ONLY). If this flag is switched on,
            then it will NOT scan any product but will only print the reports. But you still can provide -p flag to
            provide selected products for which reports will be generated.
    -p      List of products you want to scan: "(${PRODUCTS[@]})"
--------------------------
-- Required Arguments
--------------------------
    None

--------------------------
-- Example(s)
--------------------------
1. Run scanning for EPAS-12 only:
--------------------------
./wrapper.sh -p epas-12

2. Run scanning for all EPAS-12 & EPAS-13 :
--------------------------
./wrapper.sh -p "epas-12 epas-13"

3. Run scanning for all supported products:
--------------------------
./wrapper.sh

4. Generate default report for EPAS-12 only:
--------------------------
./wrapper.sh -p epas-12 -g

2. Generate default reports for EPAS-12 & EPAS-13 :
--------------------------
./wrapper.sh -p "epas-12 epas-13" -g

3. Generate default reports for scanning for all supported products:
--------------------------
./wrapper.sh -g


EOF
}

PrintErrorLogSummary()
{
    MessageHeading "Error Log Summary"

    if [[ -f $ERROR_LOG_SUMMARY ]];
    then
        cat $ERROR_LOG_SUMMARY
    else
        Message $MSG_INFO "[Function: $FUNCNAME] No errors are captured in error summary file."

        if [[ $RETURN_CODE -gt 0 ]];
        then
            Message $MSG_INFO "[Function: $FUNCNAME] There are [$RETURN_CODE] errors in total. Please check the log file for details."
        else
            Message $MSG_INFO "[Function: $FUNCNAME] No errors found."
        fi
    fi
}

PrintWarningLogSummary()
{
    MessageHeading "Warnings Log Summary"

    if [[ -f $WARNING_LOG_SUMMARY ]];
    then
        cat $WARNING_LOG_SUMMARY
    else
        Message $MSG_INFO "[Function: $FUNCNAME] No warning are captured in warning summary file."
    fi
}

ProcessProductFile()
{
    ProductFile="$1"
    returnCode=0

    ExecuteCommand "cd $WD"
    source $ProductFile
    returnCode=$?

    return $returnCode
}
SetLocation()
{
    DNS=$(grep -w 172 /etc/resolv.conf | cut -f2,3 -d .)

    if [[ $DNS ==  "24.32" || $DNS == "16.0" ]];
    then
        export DNS_EXT="ox.uk.enterprisedb.com"
        export LOCATION="UK"
    elif [[ $DNS ==  "24.34" || $DNS == "19.0" || $DNS == "19.5" ]];
    then
        export DNS_EXT="pn.in.enterprisedb.com"
        export LOCATION="IN"
    elif [[ $DNS == "24.36" ]];
    then
        export DNS_EXT="isb.pk.enterprisedb.com"
        export LOCATION="PK"
    elif [ -z “$DNS” ]
    then
        export DNS_EXT="unknownlocation"
        echo "Unable to determine host location. Check /etc/resolv.conf"
        exit 1
    fi
}

# Check options passed in.
while getopts "hg p:f:s:" OPTION
do
    case $OPTION in
        h)
            usage
            exit 1
            ;;
        f)
            export PLATFORM_TO_SCAN="$OPTARG"
            ;;
        g)
            export GENERATE_REPORTS=true
            ;;
        s)
            export PPAS_VERSION="$OPTARG"
            ;;
        p)
            read -a USER_PRODUCTS <<< "$OPTARG"
            ;;
        ?)
            usage
            exit 0
            ;;
    esac
done
#Remove old error log summary
ExecuteCommand "rm -f $ERROR_LOG_SUMMARY"

#Remove old clone warning log summary
ExecuteCommand "rm -f $WARNING_LOG_SUMMARY"

# Set Location specific variables
SetLocation

# Initialize folder structure
InitializeFolderStructure

# Print the global variables
ShowGlobalVariables

RETURN_CODE=0

# echo PPAS_VERSION===>$PPAS_VERSION

# Check if report generation mode in
if $GENERATE_REPORTS;
then
    MessageHeading "Report Generation Mode..."
    ExecuteCommand "rm -fr $REPORTS_HOME/*"
else
    # Scanning Products
    MessageHeading "Scanning Products..."
fi

if [[ ${#USER_PRODUCTS[@]} -gt 0 ]];
then
    PRODUCTS=("${USER_PRODUCTS[@]}")
fi

for i in ${!PRODUCTS[@]} ; do
    Message $MSG_INFO "Starting for ${PRODUCTS[$i]}"

    CURRENT_PRODUCT_SCANNED="${PRODUCTS[$i]}.sh"
    (ProcessProductFile $CURRENT_PRODUCT_SCANNED)
    RETURN_VALUE=$?
    RETURN_CODE=$(expr $RETURN_VALUE + $RETURN_CODE)
    CURRENT_PRODUCT_SCANNED=""
done

#Print all captured warnings in cloning packages from builds or redux store
PrintWarningLogSummary

#Print all captured Errors at end of Scan log file
PrintErrorLogSummary

if [[ $RETURN_CODE -gt 0 && "$GENERATE_REPORTS" = false ]];
then
    Message $MSG_INFO "Scanning Problems in: $RETURN_CODE products. Check logs"
fi
#Upload Generated BD reports to builds.enterprisedb.com
if $GENERATE_REPORTS;
then
    if [[ $RETURN_CODE -gt 0 ]];
    then
        Message $MSG_INFO "Reports printing problems in: $RETURN_CODE products. Check log"
    fi

    UploadReportsToBuilds
fi

MessageHeading "Finished..."
## END SCRIPT
exit $RETURN_CODE