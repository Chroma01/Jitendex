#!/usr/bin/env fish
# Copyright (c) 2025 Stephen Kraus
# SPDX-License-Identifier: AGPL-3.0-or-later

set file_name 'examples.utf'
set project_dir (realpath (status dirname))

set file_fetch_script \
    "$project_dir"/../Data/edrdg-dictionary-archive/scripts/get_file_by_date.fish

set examples_file (
    fish "$file_fetch_script" \
        --file "$file_name" \
        --latest)
or return 1

dotnet run \
    --project "$project_dir" \
    --configuration 'Release' \
    -- "$examples_file"
