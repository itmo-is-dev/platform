#!/bin/bash

source scripts/common-scripts.sh

function _find_project_references_by_short_name {
  cat | short_name_to_csproj_path | while IFS= read -r csproj_path
  do
    echo "$csproj_path"
    
    grep -E 'ProjectReference' "$csproj_path" \
      | get_tag_attribute_value 'Include' \
      | csproj_path_to_unix_path \
      | csproj_path_to_short_name \
      | _find_project_references_by_short_name  
  done
}

# shellcheck disable=SC2120
function find_project_references_by_short_name {
  cat | _find_project_references_by_short_name | sort | uniq | csproj_path_to_short_name
}

function find_package_references_by_short_name {
  cat \
    | find_project_references_by_short_name \
    | short_name_to_csproj_path \
    | xargs -I {} grep 'PackageReference' {} \
    | get_tag_attribute_value 'Include' \
    | sort \
    | uniq
}

function _find_package_projects {
  find . -type f -name '*.csproj' | while IFS= read -r csproj_path
  do
    
    is_package=$(grep -Ec '<IsPlatformPackage>true</IsPlatformPackage>' "$csproj_path")
    
    if [[ "$is_package" -eq 0 ]]
    then
      continue
    fi
    
    echo "$csproj_path" | csproj_path_to_short_name
    
  done
}

function find_package_projects {
   _find_package_projects
}