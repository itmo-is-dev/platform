#!/bin/bash

source ./scripts/common-scripts.sh

function change_package_patch_version {
  short_name="$1"
  patch_version="$2"
  
  package_name=$(echo "$short_name" | short_name_to_full_name)
  
  property_group_line_number=$(grep -n \'"$package_name"\' Directory.Platform.props | awk -F : '{ print $1 }')
  
  if [[ -z "$property_group_line_number" ]]
  then
    >&2 echo 'failed to find property group for package = '"$package_name"
    exit 1
  fi
  
  patch_version_line_number=$(( property_group_line_number + 3 ))
  current_patch_version=$(sed -n "$patch_version_line_number"p Directory.Platform.props | grep -Eo '[0-9]+')
  
  if [[ "$OSTYPE" == 'darwin'* ]]
  then
    sed -i '' "$patch_version_line_number"s/"$current_patch_version"/"$patch_version"/ 'Directory.Platform.props'
  else
    sed -i "$patch_version_line_number"s/"$current_patch_version"/"$patch_version"/ 'Directory.Platform.props'
  fi
}