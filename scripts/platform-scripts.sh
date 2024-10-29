#!/bin/bash

source ./scripts/common-scripts.sh

function change_package_patch_version {
  short_name="$1"
  patch_version="$2"
  
  csproj_path=$(echo "$short_name" | short_name_to_csproj_path)
  
  platform_version_line_number=$(grep -n '<PropertyGroup Label="PlatformVersion">' "$csproj_path" | awk -F : '{ print $1 }')
  
  if [[ -z "$platform_version_line_number" ]]
  then
    >&2 echo 'failed to find platform version property group for package = '"$short_name"
    exit 1
  fi
  
  patch_version_line_number=$(( platform_version_line_number + 3 ))
  current_patch_version=$(sed -n "$patch_version_line_number"p "$csproj_path" | grep -Eo '[0-9]+')
  
  if [[ "$OSTYPE" == 'darwin'* ]]
  then
    sed -i '' "$patch_version_line_number"s/"$current_patch_version"/"$patch_version"/ "$csproj_path"
  else
    sed -i "$patch_version_line_number"s/"$current_patch_version"/"$patch_version"/ "$csproj_path"
  fi

}