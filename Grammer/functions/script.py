import os, sys
import datetime
import pyaudio

help_menu = """      Help menu

WARNING: When working with files, it is better to use CMD.
#
# cp First file New folder - don't work
# cp First\sfile New\sfolder - work
#
# by reallyShould

Basic:
    SHUTDOWN
    REBOOT
    SLEEP
    LOCK
    screen - screenshot
Other:
    ls - list files
    mkdir - create directory
    cd <path> - change dir
    cp <file path> <to path> - copy file
    mv <file path> <to path> - move file
    rm <path> - delete folder or files 
    start - open file
    getfile <path> - file from PC (limited)
    cmd <command> - windows cmd (limited)
    cmd2 - cmd with output (testing)
    info <file path> - give some information about the file 
    drop file to load on PC (unstable)
    link <link> - open link in Explorer 
    web <string> - search in web 
    cat <path> - read text file
    dpkg <path> - unpack (work with .zip) 
    playsound <path> - testing
    whoami - get your Telegram ID
Voice:
    Send voice message to stream on PC
    `voice` - start recording
    `voice setseconds` <int>
    `voice setchunk` <int>
    `voice setrate` <int> (48000, 44100)
    `voice setchanells` <int>
    `voice setdevice` <int>
    `voice settings` - list of values
    `voice list` - list of devices
"""

# info about user and bot
user = None
ver = '1.2.0b1'
UserName = os.getlogin()
work_directory = '\\'.join(sys.argv[0].split('\\')[0:-1])
dateTmp = datetime.datetime.now()
last_restart = str(dateTmp.day) + '-' + str(dateTmp.month) + '-' + str(dateTmp.year) + ' ' \
               + str(dateTmp.hour) + ':' + str(dateTmp.minute) + ':' + str(dateTmp.second)

#AUDIO
chunk = None
channels = None
rate = None
record_seconds = None
device = None
WAVE_OUTPUT_FILENAME = "voice.wav"
form = pyaudio.paInt16

def start_message():
    dateTmp = datetime.datetime.now()
    date = str(dateTmp.day) + '-' + str(dateTmp.month) + '-' + str(dateTmp.year) + ' ' + str(dateTmp.hour) + ':' \
           + str(dateTmp.minute) + ':' + str(dateTmp.second)
    help_message = f'''{help_menu}
==========
Date: {date}
==========
Script path: {work_directory}
==========
User name: {UserName}
==========
Last restart: {last_restart}
==========
Version: {ver}
==========
'''
    return help_message


def init():
    global user
    global chunk
    global channels
    global rate
    global record_seconds
    global device

    try:
        with open(f'{work_directory}\\login.txt', 'r') as fi:
            user = int(fi.read())
    except:
        pass

    try:
        os.chdir(f'C:\\Users\\{os.getlogin()}\\Desktop')
    except:
        print('Not found desktop')

    try:
        with open(f'{work_directory}\\config.txt', 'r') as f:
            tmpL = f.readline()
            tmpL = tmpL.split(' ')
            chunk = int(tmpL[0])
            channels = int(tmpL[1])
            rate = int(tmpL[2])
            record_seconds = int(tmpL[3])
            device = int(tmpL[4])
    except FileNotFoundError:
        print('No such file config.txt\nCreate new...')
        chunk = 1024
        channels = 1
        rate = 48000
        record_seconds = 300
        device = 1
        with open(f'{work_directory}\\config.txt', 'a+') as f:
            f.write(f'{1024} {1} {48000} {300} {1}')


def splitter(text):
    text = text.replace('  ', ' ')
    tmp = text.strip().split(' ')

    out = []
    for i in tmp:
        i = i.replace('\\s', ' ')
        out.append(i)
    return out

def getToken(path=f'{work_directory}\\token.txt'):
    try:
        return open('token.txt', 'r').read()
    except:
        return None