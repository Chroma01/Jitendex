#!/usr/bin/env fish

set project_dir (status dirname)

set kanjivg_kanji_dir \
    "$project_dir"/../Data/kanjivg/kanji

set cache_dir 'Jitendex/kanjivg'
if set -q XDG_CACHE_HOME
    set kanji_file_dir "$XDG_CACHE_HOME"/"$cache_dir"
else
    set kanji_file_dir "$HOME"/'.cache'/"$cache_dir"
end

# Use the current KanjiVG commit ID as the name of the cached archive.
set kanji_file_name \
    (git rev-parse --verify --short HEAD:(dirname "$kanjivg_kanji_dir")).tar.br

set kanji_file_path \
    "$kanji_file_dir"/"$kanji_file_name"

if not test -e "$kanji_file_path"
    if test -d "$kanji_file_dir"
        rm -r "$kanji_file_dir"
    end
    mkdir -p "$kanji_file_dir"

    cd "$kanjivg_kanji_dir"
    tar -c *.svg | brotli -4 > "$kanji_file_path"
    cd "$project_dir"
end

time dotnet run \
    --project "$project_dir" \
    --configuration Release \
    -- "$kanji_file_path"
