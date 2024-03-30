import os
import webbrowser
from mss import mss
import zipfile
import shutil
from distutils.dir_util import copy_tree

SYSTEM = {
    'LOCK': 'Rundll32.exe user32.dll,LockWorkStation',
    'SHUTDOWN': 'shutdown /s /t 00',
    'REBOOT': 'shutdown /r /t 00',
    'SLEEP': 'rundll32 powrprof.dll,SetSuspendState 0,1,0'
}

# show current directory files
def ls(path=os.getcwd()):
    print(f'[START] ls {path}')
    out = f'Folder: `{os.getcwd()}`\nElements: {len(os.listdir(path))}\n\n'
    files = os.listdir()
    print(f'[LOG] files: {files}')
    for i in files:
        out += f'`{i}`\n==========\n'
    try:
        print(f'[DONE] {path}')
        return out
    except:
        return 'Huge list'

# copy file/folder fromPath -> toPath
def copy(fromPath, toPath):
    print(f'[START] Copy {fromPath} {toPath}')
    try:
        if os.path.isdir(fromPath):
            print(f'[LOG] Path: {fromPath} is dir')
            tmpPosition = os.getcwd()
            print(f'[LOG] Curent position is {tmpPosition}')
            os.chdir(fromPath)
            fromPath = os.getcwd()
            os.chdir(tmpPosition)

            os.chdir(toPath)
            toPath = os.getcwd()
            os.chdir(tmpPosition)

            os.chdir(toPath)
            tmp = toPath + '/' + fromPath.split('\\')[-1]
            copy_tree(fromPath, tmp)
            os.chdir(tmpPosition)
            print(f'[DONE] Copy {fromPath} to {toPath} DONE')
            return f'Copy {fromPath} to {toPath}'
        else:
            print(f'[LOG] Path: {fromPath} not dir')
            shutil.copy(fromPath, toPath)
            print(f'[DONE] Copy "{fromPath}" to "{toPath}"')
            return f'Copy {fromPath} to {toPath}'
    except Exception as err:
        print(f'[ERROR] {err}')
        return err

# remove file/folder
def rm(path):
    print(f'[START] Remove {path}')
    try:
        if os.path.isdir(path) == True:
            rnpath = os.getcwd()
            os.chdir(path)
            filesForDel = os.listdir()
            for i in filesForDel:
                rm(i)
            os.chdir(rnpath)
            os.rmdir(path)
            print(f'[DONE] {path}')
            return f'"{path}" has been deleted'
        else:
            os.remove(path)
            print(f'[DONE] {path}')
            return f'"{path}" has been deleted'
    except Exception as err:
        print(f'[ERROR] {err}')
        return err

# move file fromPath -> toPath
def mv(fromPath, toPath):
    print(f'[START] move {fromPath} {toPath}')
    try:
        copy(fromPath, toPath)
        rm(fromPath)
        print(f'[DONE] move {fromPath} {toPath}')
        return f'Done move from "{fromPath}" to "{toPath}"'
    except Exception as err:
        print(f'[ERROR] {err}')
        return err
    
# return screenshot
def screenshot():
    print('[LOG] screenshot')
    with mss() as sct:
        filename = sct.shot(mon=-1, output='fullscreen.png')
        return filename
    
# data and time converter
def convert_to_preferred_format(sec): 
    sec = sec % (24 * 3600)
    hour = sec // 3600 
    sec %= 3600 
    minutes = sec // 60 
    sec %= 60 
    return "%02d:%02d:%02d" % (hour - 5, minutes, sec)


def fileInfo(file):
    print(f'[LOG] File info {file}')
    return f'''
    {file}

    File size: {os.path.getsize(file)}B ({os.path.getsize(file)/1024}MB)
    Modify time: {convert_to_preferred_format(os.path.getmtime(file))}
    Create time: {convert_to_preferred_format(os.path.getctime(file))}'''

# open link in Explorer
def link(x):
    print(f'[START] Link {x}')
    l = ['opera', 'chrome', 'chromium', 'mozilla']
    try:
        webbrowser.open_new_tab(x)
        print(f'[DONE] Link {x}')
        return True
    except Exception as err:
        print(f'[ERROR] {err}')
        return False

# search in web
def web(search):
    print(f'[START] Web {search}')
    try:
        webbrowser.open_new_tab(f'https://duckduckgo.com/?t=ffab&q={search}&ia=web')
        print(f'[DONE] Web {search}')
        return True
    except Exception as err:
        print(f'[ERROR] {err}')
        return False

# read text file
def cat(filename):
    print(f'[LOG] Read {filename}')
    with open(f'{filename}', 'r') as f:
        return f.read()

# (work with .zip) 
def unpack(file):
    arch = zipfile.ZipFile(file, 'r')
    fileName = arch.filename.replace('.zip', '').replace('.rar', '')
    os.mkdir(fileName)
    os.chdir(fileName)
    arch.extractall()
    os.chdir('..')
    print(fileName)