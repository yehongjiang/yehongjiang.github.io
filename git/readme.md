# git学习
## 1):安装完以后的一点操作  
安装完成后，还需要最后一步设置，在命令行输入：  
`$ git config --global user.name "Your Name"`  
`$ git config --global user.email "email@example.com"`  
因为Git是分布式版本控制系统，所以，每个机器都必须自报家门：你的名字和Email地址。你也许会担心，如果有人故意冒充别人怎么办？这个不必担心，首先我们相信大家都是善良无知的群众，其次，真的有冒充的也是有办法可查的。

注意git config命令的--global参数，用了这个参数，表示你这台机器上所有的Git仓库都会使用这个配置，当然也可以对某个仓库指定不同的用户名和Email地址。
## 2):git的操作
`$ mkdir learngit`	//创建一个文件夹  
`$ cd learngit`	//进入文件夹 cd ..就是返回到上一级文件夹中去，注意有空格  
`$ pwd	`	//显示当前目录  
`$ git init` 		//通过这个命令把当前目录编程GIT可以管理的仓库  
`$ ls -ah`		//可以看见隐藏目录如.git目录  
`$ git add readme.txt`	//添加一个readme.txt文件，之前必须手动创建该文件并添加内容  
`$ git commit -m "wrote a readme file"` 	 	//把文件提交到仓库，-m后面输入的是本次提交的说明，不想输入-m "xxx"行不行？确实有办法可以这么干  
：）为什么Git添加文件需要add，commit一共两步呢？因为commit可以一次提交很多文件，所以你可以多次add不同的文件，比如： 

- `$ git add file1.txt`
-	`$ git add file2.txt file3.txt`
-	`$ git commit -m "add 3 files." ` 

`$ git status`	//命令可以让我们时刻掌握仓库当前的状态，上面的命令输出告诉我们，readme.txt被修改过了，但还没有准备提交的修改。  
`$ git diff readme.txt` 	//可以看到修改了什么地方  
：）提交修改和提交新文件是一样的两步先add 再commit  
`$ git log	`	//可以查看修改日志，一大串类似1094adb...的是commit id（版本号16进制表示）  
`$ git log --pretty=oneline` 	//简化输出，在Git中，用HEAD表示当前版本  
`$ git reset --hard HEAD^` 	//退回到上一个版本，HEAD~100往上一百个版本  
：）后悔了怎么办，只要上面的命令行窗口还没有被关掉，你就可以顺着往上找啊找啊，找到那个append GPL的commit id是1094adb...，于是就可以指定回到未来的某个版本：  
`$ git reset --hard 1094a`	//版本号写前几位就行  
`$ cat readme.txt`	//可以看到文件里面的内容  
：）现在，你回退到了某个版本，关掉了电脑，第二天早上就后悔了，想恢复到新版本怎么办？找不到新版本的commit id怎么办？  
在Git中，总是有后悔药可以吃的。  
`$ git reflog`  
`$ vi readme.txt`	//可以进入文本进行编辑，但是退出会多出一个swp文件是按ctrl+z所造成的也不知道怎么正常退出再说吧  
## 3）：工作区和暂存区
：）工作区-就是你在电脑里能看到的目录，比如git_learning文件夹  
：）版本库（repository）-工作区有一个隐藏目录.git，这个不算工作区，而是Git的版本库。Git的版本库里存了很多东西，其中最重要的就是称为stage（或者叫index）的暂存区，还有Git为我们自动创建的第一个分支master，以及指向master的一个指针叫HEAD。  
：）前面讲了我们把文件往Git版本库里添加的时候，是分两步执行的：
第一步是用git add把文件添加进去，实际上就是把文件修改添加到暂存区；  
第二步是用git commit提交更改，实际上就是把暂存区的所有内容提交到当前分支。  
因为我们创建Git版本库时，Git自动为我们创建了唯一一个master分支，所以，现在，git commit就是往master分支上提交更改。
你可以简单理解为，需要提交的文件修改通通放到暂存区，然后，一次性提交暂存区的所有修改。  
## 3):管理修改
：）为什么Git比其他版本控制系统设计得优秀，因为Git跟踪并管理的是修改，而非文件。  
## 4):撤销修改
`$ git checkout -- readme.txt`		//就是让这个文件回到最近一次git commit或git add时的状态。丢弃工作区的状态  
：）若已经add了那就要用前面说的reset来回到上一个版本    

## 5):删除文件
`$ rm test.txt`	//一般情况下，你通常直接在文件管理器中把没用的文件删了，或者用rm命令删了：  
：）现在你有两个选择，一是确实要从版本库中删除该文件，那就用命令git rm删掉，并且git commit：  
`$ git rm test.txt`	//再这之前test文件要已经被提交  
`$ git commit -m "remove test.txt"`  
：）先手动删除文件，然后使用git rm <file>和git add<file>效果是一样的。  
：）另一种情况是删错了，因为版本库里还有呢，所以可以很轻松地把误删的文件恢复到最新版本：  
`$ git checkout -- test.txt`	//其实是用版本库里的版本替换工作区的版本，无论工作区是修改还是删除，都可以“一键还原”。  
## 6):远程仓库
：）由于你的本地Git仓库和GitHub仓库之间的传输是通过SSH加密的，所以，需要一点设置：  
：）第1步：创建SSH Key。在用户主目录下，看看有没有.ssh目录，如果有，再看看这个目录下有没有id_rsa和id_rsa.pub这两个文件，如果已经有了，可直接跳到下一步。如果没有，打开Shell（Windows下打开Git Bash），创建SSH Key：  
`$ ssh-keygen -t rsa -C "youremail@example.com"`  
：）你需要把邮件地址换成你自己的邮件地址，然后一路回车，使用默认值即可，由于这个Key也不是用于军事目的，所以也无需设置密码。  
：）如果一切顺利的话，可以在用户主目录里找到.ssh目录，里面有id_rsa和id_rsa.pub两个文件，这两个就是SSH Key的秘钥对，id_rsa是私钥，不能泄露出去，id_rsa.pub是公钥，可以放心地告诉任何人。  
：）为什么GitHub需要SSH Key呢？因为GitHub需要识别出你推送的提交确实是你推送的，而不是别人冒充的，而Git支持SSH协议，所以，GitHub只要知道了你的公钥，就可以确认只有你自己才能推送。  
## 7):添加远程库
`$ git remote add origin git@github.com:michaelliao/learngit.git`	//关联一个远程库  
`$ git push -u origin master` 	//第一次推送master分支的所有内容  
：）以上两步就可以将本地的文件推送到github上保管了，添加后，远程库的名字就是origin，这是Git默认的叫法，也可以改成别的，但是origin这个名字一看就知道是远程库。  
：）把本地库的内容推送到远程，用git push命令，实际上是把当前分支master推送到远程。由于远程库是空的，我们第一次推送master分支时，加上了-u参数，Git不但会把本地的master分支内容推送的远程新的master分支，还会把本地的master分支和远程的master分支关联起来，在以后的推送或者拉取时就可以简化命令。  
`$ git push origin master` 	//之后提交用这条语句就可以了  
## 8):从远程库克隆
`$ git clone git@github.com:michaelliao/gitskills.git`	//就把github上的库复制到本地计算机上了，经试验克隆不需要文件夹是仓库，但是不是仓库就不可以进行git操作  
`$ git pull origin master`  //是可以把github上更新的内容复制到本地的，好像是补充与替换，没有深入研究，好像是fecth与merge的集合操作  

## 9):分支管理——平行宇宙
`$ git checkout -b dev`	//创建了名为dev的分支  
：）git checkout命令加上-b参数表示创建并切换，相当于以下两条命令：   
 
- `$ git branch dev`  
- `$ git checkout dev`  

`$ git branch`	//查看当前的有哪些分支，指针正指向哪一条分支  
`$ git checkout master` 	//切换回master分支  
`$ git merge dev`	//把dev分支的工作成果合并到master分支上  
：）注意到上面的Fast-forward信息，Git告诉我们，这次合并是“快进模式”，也就是直接把master指向dev的当前提交，所以合并速度非常快。  
`$ git branch -d dev`		//删除dev分支  
：）删除本地分支：git branch -d 分支名（remotes/origin/分支名）  
      强制删本地：git branch -D 分支名  
      删除远程分支：git push origin --delete 分支名（remotes/origin/分支名）  

## 10):解决冲突
###### —合并分支往往也不是一帆风顺的：如果两个分支同时修改并提交了的话
：）用上面的merge会产生冲突，当前文件会把两次的修改都按一定格式放入文件  
：）当Git无法自动合并分支时，就必须首先解决冲突。解决冲突后，再提交，合并完成。解决冲突就是把Git合并失败的文件手动编辑为我们希望的内容，再提交。用git log --graph命令可以看到分支合并图。  
## 11):分支管理策略
：）通常，合并分支时，如果可能，Git会用Fast forward模式，但这种模式下，删除分支后，会丢掉分支信息。如果要强制禁用Fast forward模式，Git就会在merge时生成一个新的commit，这样，从分支历史上就可以看出分支信息。下面我们实战一下--no-ff方式的git merge  
`$ git merge --no-ff -m "merge with no-ff" dev`
## 12):Bug分支
###### —当你接到一个修复一个代号101的bug的任务时，很自然地，你想创建一个分支issue-101来修复它，但是，等等，当前正在dev上进行的工作还没有提交（还没做完）
`$ git stash`		//可以把当前工作现场“储藏”起来，等以后恢复现场后继续工作  
`$ git stash list`	//可以看到之前储藏的工作  
：）一是用git stash apply恢复，但是恢复后，stash内容并不删除，你需要用git stash drop来删除；  
`$ git stash pop`	//恢复的同时把stash内容也删了  
`$ git stash apply stash@{0}`	//你可以多次stash，恢复的时候，先用git stash list查看，然后恢复指定的stash  
## 13):Feature分支
###### —软件开发中，总有无穷无尽的新的功能要不断添加进来。
`$ git branch -D feature-vulcan`	//若一分支还未合并那么git会提醒无法合并，这时要用D强行删除  
## 14):多人协作
`$ git remote`	//查看远程库的信息  
`$ git remote -v`	//显示更加详细一些，如果没有推送权限，就看不到push的地址  
：）推送分支->`$ git push origin maste`r 或者 `$ git push origin dev`  
：）但是，并不是一定要把本地分支往远程推送，那么，哪些分支需要推送，哪些不需要呢？ 
 
- master分支是主分支，因此要时刻与远程同步；  
- dev分支是开发分支，团队所有成员都需要在上面工作，所以也需要与远程同步；  
- bug分支只用于在本地修复bug，就没必要推到远程了，除非老板要看看你每周到底-修复了几个bug；  
- feature分支是否推到远程，取决于你是否和你的小伙伴合作在上面开发。  

：）抓取分支——当你的小伙伴从远程库clone时，默认情况下，你的小伙伴只能看到本地的master分支。不信可以用git branch命令看看  
`$ git checkout -b dev origin/dev`	//你的小伙伴要在dev分支上开发，就必须创建远程origin的dev分支到本地，于是他用这个命令创建本地dev分支  
：）你的小伙伴已经向origin/dev分支推送了他的提交，而碰巧你也对同样的文件作了修改，并试图推送  
`$ git pull	`	//先用git pull把最新的提交从origin/dev抓下来，然后，在本地合并，解决冲突，再推送  
`$ git branch`	//git pull也失败了，原因是没有指定本地dev分支与远程origin/dev分支的链接，根据提示，设置dev和		origin/dev的链接  
：）因此，多人协作的工作模式通常是这样：  
	-首先，可以试图用git push origin <branch-name>推送自己的修改；  
	-如果推送失败，则因为远程分支比你的本地更新，需要先用git pull试图合并；  
	-如果合并有冲突，则解决冲突，并在本地提交；  
	-没有冲突或者解决掉冲突后，再用git push origin <branch-name>推送就能成功！  
	-如果git pull提示no tracking information，则说明本地分支和远程分支的链接关系没有创建，用命令git branch --		set-upstream-to <branch-name> origin/<branch-name>。  
	-这就是多人协作的工作模式，一旦熟悉了，就非常简单。  
## 15):Rebase
###### —为什么Git的提交历史不能是一条干净的直线
`$ git rebase`	//没有仔细体验，等以后用到了再详细学习吧  

## 16):标签管理
###### 相当于版本库的一个快照，tag就是一个让人容易记住的有意义的名字，方便版本查找
`$ git tag v1.0`	//在相应的分支上打上标签  
`$ git tag`		//可以查看所有标签  
`$ git log --pretty=oneline --abbrev-commit` 	//若是忘了打标签找到相应提交版本$ git tag v0.9 f52c633  
`$ git show v0.9`	//查看标签信息，标签不是按时间顺序列出，而是按字母排序的。  
`$ git tag -a v0.1 -m "version 0.1 released" 1094adb`	//还可以创建带有说明的标签，用-a指定标签名，-m指定说明文字  
`$ git tag -d v0.1`	//删除标签  
`$ git push origin v1.0`	//推送标签远程  
`$ git push origin --tags`	//一次性推送全部尚未推送到远程的本地标签
：）如果标签已经推送到远程，要删除远程标签就麻烦一点，先从本地删除  
`$ git tag -d v0.9	$ git push origin :refs/tags/v0.9`
