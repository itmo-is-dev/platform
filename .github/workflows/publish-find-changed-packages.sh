#!/bin/bash

package_projects=$(echo "$PACKAGE_PROJECTS_JSON" | jq -r 'values[]' | tr '\n' ' ')

for package_project in ${package_projects}
do
  dependent_paths=$(.github/workflows/publish-find-csproj-dependencies.sh "${package_project}" | sort | uniq)

  for dependent_path in ${dependent_paths}
  do
    changes_count=$(echo "$ALL_CHANGED_FILES" | grep -Ec "${dependent_path}")

    if [[ ! "$changes_count" -eq 0 ]]
    then
      echo "${package_project}"
      break
    fi
  done
done