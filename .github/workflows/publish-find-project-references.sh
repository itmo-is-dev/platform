#!/bin/bash

function find_dependencies {
  project="$1"

  csproj_path=$(find . -type d -name Itmo.Dev.Platform."$project")/Itmo.Dev.Platform."$project".csproj
  dirname "${csproj_path}" | sed 's/^.\///'
  
  dependent_project_paths=$(grep -E 'ProjectReference' "${csproj_path}" | grep -Eo '".*"' | tr -d '\"' | tr '\\' '/'| tr '\n' ' ')
  
  for dependency_project in ${dependent_project_paths}
  do
    dependency_project_dir=$(basename "$(dirname "${dependency_project}")" | sed 's/Itmo.Dev.Platform.//')
    find_dependencies "$dependency_project_dir"
  done
}

find_dependencies "$1"