#!/bin/bash

package_projects=$(echo "$PACKAGE_PROJECTS_JSON" | jq -r 'values[]' | tr '\n' ' ')

for package_project in ${package_projects}
do
  project_references=$(.github/workflows/publish-find-project-references.sh "${package_project}" | sort | uniq)

  for dependent_path in ${project_references}
  do
    changes_count=$(echo "$ALL_CHANGED_FILES" | grep -Ec "${dependent_path}")

    if [[ ! "$changes_count" -eq 0 ]]
    then
      echo "${package_project}"
      break
    fi
  done
done

if [[ ! -z "${ALL_CHANGED_DEPENDENCIES}" ]]
then
  for package_project in ${package_projects}
  do
    package_references=$(.github/workflows/publish-find-package-references.sh "${package_project}" | sort | uniq)
    
    for package_reference in ${package_references}
    do
      changes_count=$(echo "${ALL_CHANGED_DEPENDENCIES}" | grep -Ec "${package_reference}")
      
      if [[ ! "$changes_count" -eq 0 ]]
      then
        echo "${package_project}"
        break
      fi
      
    done
    
  done
fi