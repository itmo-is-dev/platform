#!/bin/bash

source ./scripts/project-scripts.sh

function find_changed_projects {
  changes=$(cat)
  
  find_package_projects | while read -r project
  do
    >&2 echo 'checking changes for project = '"$project"
    
    echo "$project" | find_project_references_by_short_name | short_name_to_dir_path | while IFS= read -r dependency
    do
      >&2 echo "\t"'checking changes for project dependency = '"$dependency"
      changed=$(echo "$changes" | grep -Ec "$dependency")
      
      if [[ ! "$changed" -eq 0 ]]
      then
        echo "$project"
        break
      fi
    done
  done
}

function find_changed_dependencies {
    changed_dependencies=$(cat)
    
    find_package_projects | while read -r project
    do
      >&2 echo 'checking package changes for project = '"$project"
      echo "$project" | find_package_references_by_short_name | while read -r package_referece
      do
        >&2 echo "\t"'checking changes for package = '"$package_referece"
        
        changed=$(echo "$changed_dependencies" | grep -Ec "$package_referece")
        
        if [[ ! "$changed" -eq 0 ]]
        then
          echo "$project"
          break 
        fi
      done
    done
}