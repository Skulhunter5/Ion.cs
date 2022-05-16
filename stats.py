from os import listdir
from os.path import isfile, isdir, join

dirsToIngore = [r".\.vscode", r".\bin", r".\.git", r".\obj"]
endingsToIgnore = [".gitignore", ".gitattributes", ".csproj", ".md", ".txt"]

def filterFiles(file):
    for ending in endingsToIgnore:
        if(file.endswith(ending)):
            return False
    return True

def filterDirs(dir):
    for d in dirsToIngore:
        if(dir == d):
            return False
    return True

def filterFiles_cs(file):
    return file.endswith(".cs")

def filterFiles_ions(file):
    return file.endswith(".ions")

def collect(path):
    files = [join(path, f) for f in listdir(path) if isfile(join(path, f))]
    files = list(filter(filterFiles, files))
    dirs = [join(path, d) for d in listdir(path) if isdir(join(path, d))]
    dirs = list(filter(filterDirs, dirs))
    for dir in dirs:
        files.extend(collect(dir))
    return files

def countLines(filepath):
    return sum(1 for _ in open(filepath))

def countLinesOfArray(files):
    s = 0
    for file in files:
        s += countLines(file)
    return s

def getEnding(filename):
    for i in range(len(filename)-1, -1, -1):
        if(filename[i] == '.'):
            return filename[i:]
    return None

def listAll():
    endings = {}
    for file in collect("."):
        ending = getEnding(file)
        if(ending in endings):
            endings[ending] += countLines(file)
        else:
            endings[ending] = countLines(file)

    for ending in endings:
        print("'" + ending + "': " + str(endings[ending]))

def listCustom():
    total = 0

    csLines = countLinesOfArray(list(filter(filterFiles_cs, collect(r".\src"))))
    total += csLines
    print("'*.cs': " + str(csLines))

listCustom()
