#!/bin/sh

git add .
echo Commit Message:
read -p 'Commit Message: ' commitmessage
git commit -am "$commitmessage"
git push


