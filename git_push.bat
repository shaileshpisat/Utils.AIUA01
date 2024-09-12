@echo off
echo usage :  ./git_push "text comments"

git add .
IF [%1] == [] GOTO NoComment
git commit -m %1
git push

:NoComment
git commit -m "no comments"
git push