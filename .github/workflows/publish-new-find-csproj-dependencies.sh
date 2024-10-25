#!/bin/bash

function find_dependencies {
  local project_dir="$1"

  csproj_path=$(find . -type d -name "$project_dir")/"$project_dir".csproj
  dirname "${csproj_path}"
  
  dependent_project_paths=$(grep -E 'ProjectReference' "${csproj_path}" | grep -Eo '".*"' | tr -d '\"' | tr '\\' '/'| tr '\n' ' ')
  
  for dependency_project in ${dependent_project_paths}
  do
    dependency_project_dir=$(basename "$(dirname "${dependency_project}")")
    find_dependencies "$dependency_project_dir"
  done
}

find_dependencies "$1"