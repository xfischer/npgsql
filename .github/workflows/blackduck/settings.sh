#!/bin/bash

# Base Folders and Paths (Default Values)
#------------------------
export SCAN_BASE_DIR="/home/buildfarm/redux-scanner"
export SCRIPTS_HOME=$SCAN_BASE_DIR/scripts/redux-scanner
export PRODUCTS_HOME=$SCAN_BASE_DIR/products
export REPORTS_HOME=$SCAN_BASE_DIR/reports
export LOGS_HOME_RELATIVE=logs
export LOGS_HOME=$SCAN_BASE_DIR/$LOGS_HOME_RELATIVE

#Following variables will be updated in each product's settings(PRODUCT-VERSION.sh) file.
export PRODUCT_FOLDER=
export SOURCE_FOLDER=
export INSTALLATION_FOOTPRINT_FOLDER=
export BINARIES_FOLDER=

#Synopsys Detect Setting Used
export BLACKDUCK_DETECT_JAR="$SCAN_BASE_DIR/jars/synopsys-detect-7.13.2.jar"
export BLACKDUCK_DETECT_DOCKER_JAR="$SCAN_BASE_DIR/jars/blackduck-docker-inspector-9.0.1.jar"
export BLACKDUCK_URL=https://enterprisedb.app.blackduck.com/
export BLACKDUCK_DETECT_CLEANUP=true
export BLACKDUCK_TRUST_CERT=true
export BLACKDUCK_DETECT_SEARCH_DEPTH="25"
export BLACKDUCK_DETECT_EXCLUDED_DETECTOR_TYPES="cpan,nuget,git"
export BLACKDUCK_DETECT_TOOLS="DETECTOR,SIGNATURE_SCAN,BINARY_SCAN"
export BLACKDUCK_PARALLEL_PROCESSOR="-1"
export BLACKDUCK_DETECTOR_SEARCH_EXCLUSION_PATTERNS="__pycache__,_internal,nbproject"

# Reports generating specific settings
export GENERATE_REPORTS=false
export BLACKDUCK_DETECT_RISK_REPORT_PDF=true
export BLACKDUCK_DETECT_RISK_REPORT_PDF_PATH=$REPORTS_HOME
export BLACKDUCK_DETECT_NOTICES_REPORT=true
export BLACKDUCK_DETECT_NOTICES_REPORT_PATH=$REPORTS_HOME
export BLACKDUCK_DETECT_REPORT_TIMEOUT=900

# Generic Folders Name
#------------------------
export SOURCE_FOLDER_RELATIVE=source
export INSTALLATION_FOOTPRINT_FOLDER_RELATIVE=installation_footprint
export BINARIES_FOLDER_RELATIVE=binaries

# Used in autorun.sh
# There will be a sepaprate log files for each run
# CreateLogFile() will create a log file in $LOGS_HOME.
export LOG_FILE=
export ERROR_LOG_SUMMARY="$LOGS_HOME/Error-Summary.log"
export WARNING_LOG_SUMMARY="$LOGS_HOME/Warning-Summary.log"

# Servers
#------------------------
export REMOTE_BUILDS=builds.enterprisedb.com
export REMOTE_REPORTS_FOLDER="/mnt/builds/daily-builds/blackduck-reports"
# LOG Levels
#------------------------
export MSG_DEBUG=0
export MSG_EMPTY=1
export MSG_INFO=2
export MSG_WARN=3
export MSG_TEST=4
export MSG_TEST_VALUE=5
export MSG_CRITICAL=6
export MSG_ERROR=7
export MSG_HEADING=8

# Networl|Location Global Variables
#------------------------
export DNS=
export DNS_EXT=
export LOCATION=

# Current Date (to make sure we use packages from builds for a given date )
#------------------------
export CURRENT_DATE=`date +%Y-%m-%d -d "1 day ago"`

#YARN and NPM related setttings for scan
export YARN_PATH="/usr/bin/yarn"
export YARN_PRODUCTUCTION_ONLY="true"
export NPM_PATH="/usr/bin/npm"
export NPM_INCLUDE_DEV_DEPENDENCIES="false"
export NPM_ARGUMENTS="\"npm install\""

# Number of days to search from current date, to find the nighlty builds on
# builds.enterprisedb.com. Back Branches are not actively monitored or looked
# after on daily basis so even if we find a single build in last X number of
# days, it will fulfil our need.
export DAYS_TO_SEARCH_OLD_BUILDS=7

# Supported platforms
export PLATFORM_LIST="windows-x64 windows-x32"