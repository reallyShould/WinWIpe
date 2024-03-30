import os
import telebot
import functions.script as script
import functions.system as system
import functions.audio as audio
import subprocess
from threading import Thread

pathToToken = f'{os.getcwd()}\\token.txt'
print(pathToToken)

token = script.getToken(pathToToken)
bot = telebot.TeleBot(token)

script.init()

print('\tBOT STARTED\n')

try:
    bot.send_message(script.user, f'''
    PC Started
    ==========
    Script path: `{script.work_directory}`
    ==========
    User name: `{script.UserName}`
    ==========
    Path: `{os.getcwd()}`
    ==========
    Version: {script.ver}
    ==========''', parse_mode='Markdown')
except:
    pass


@bot.message_handler(commands=['start', 'help', 'info'])
def start_message(message):
    bot.send_message(message.from_user.id, script.start_message(), parse_mode='Markdown')
    print('[LOG] Start message')
    if not os.path.exists(f'{script.work_directory}\\login.txt'):
        with open(f'{script.work_directory}\\login.txt', 'a+') as fi:
            fi.write(str(message.from_user.id))
            fi.close()


@bot.message_handler(content_types=['voice'])
def audio_mes(message):
    try:
        print("Audio detect")
        file_info = bot.get_file(message.voice.file_id)
        downloaded_file = bot.download_file(file_info.file_path)
        src = f'{script.work_directory}\\voice_mes.wav'
        with open(src, 'wb') as new_file:
            new_file.write(downloaded_file)
        p = Thread(target=audio.play_sound, args=(f'{script.work_directory}\\voice_mes.wav',))
        p.start()
        bot.reply_to(message, f"Good {script.work_directory}")
    except Exception as err:
        print(f'Error({err})')
        bot.reply_to(message, f"Error {err}")
    

# load file on PC
@bot.message_handler(content_types=['document'])
def doc_send(message):
    try:
        file_info = bot.get_file(message.document.file_id)
        downloaded_file = bot.download_file(file_info.file_path)
        src = f'{os.getcwd()}\\{message.document.file_name}'
        with open(src, 'wb') as new_file:
            new_file.write(downloaded_file)
        bot.reply_to(message, f"Good {os.getcwd()}")
        print(f'[LOG] Upload done, {message.document.file_id}')
    except Exception as err:
        bot.send_message(message.chat.id, err)
        print(f'[LOG] Upload fail, {message.document.file_id}')


@bot.message_handler(content_types=['text'])
def text(message):
    mes = script.splitter(message.text)
    command = mes[0].lower()
    # System commands
    try:
        os.system(system.SYSTEM[mes[0].upper()])
    except:
        pass

    # Voice control
    if command == 'voice':
        if len(mes) == 1:
            try:
                audio.streaming()
                voice = open(f'{os.getcwd()}\\{script.WAVE_OUTPUT_FILENAME}', 'rb')
                bot.send_audio(message.chat.id, voice)
                voice.close()
                os.remove(f'{os.getcwd()}\\{script.WAVE_OUTPUT_FILENAME}')
                # Just start streaming
            except Exception as err:
                bot.send_message(message.chat.id, err)

        elif mes[1].lower() == 'setseconds' and len(mes) == 3:
            try:
                script.record_seconds = int(mes[2])
                bot.send_message(message.chat.id, 'OK')
            except Exception as err:
                bot.send_message(message.chat.id, err)

        elif mes[1].lower() == 'setchunk' and len(mes) == 3:
            try:
                script.chunk = int(mes[2])
                bot.send_message(message.chat.id, 'OK')
            except Exception as err:
                bot.send_message(message.chat.id, err)

        elif mes[1].lower() == 'setchanells' and len(mes) == 3:
            try:
                script.channels = int(mes[2])
                bot.send_message(message.chat.id, 'OK')
            except Exception as err:
                bot.send_message(message.chat.id, err)

        elif mes[1].lower() == 'setrate' and len(mes) == 3:
            try:
                script.rate = int(mes[2])
                bot.send_message(message.chat.id, 'OK') 
            except Exception as err:
                bot.send_message(message.chat.id, err)

        elif mes[1].lower() == 'setdevice' and len(mes) == 3:
            try:
                script.device = int(mes[2])
                bot.send_message(message.chat.id, 'OK')
            except Exception as err:
                bot.send_message(message.chat.id, err)

        elif mes[1].lower() == 'settings' and len(mes) == 2:
            tmp = f'''    Settings
            CHUNK = {script.chunk}
            CHANELLS = {script.channels}
            RATE = {script.rate}
            SECONDS = {script.record_seconds}
            FILENAME = {script.WAVE_OUTPUT_FILENAME}
            DEVICE = {script.device}
            FORMAT = {script.form}
            '''
            bot.send_message(message.chat.id, tmp)

        elif mes[1].lower() == 'list' and len(mes) == 2:
            try:
                bot.send_message(message.chat.id, audio.device_list())
            except Exception as err:
                bot.send_message(message.chat.id, err)

        else:
            bot.send_message(message.chat.id, 'Something wrong')
        audio.savepreset()


    elif command == 'whoami':  # Give your Telegram ID
        bot.send_message(message.chat.id, message.chat.id)
        print(f'ID: {message.chat.id}')


    elif command == 'info':
        try:
            if len(mes) == 2:
                bot.send_message(message.chat.id, system.fileInfo(mes[1]))
                print(f'File info: {mes[1]}')
            else:
                bot.send_message(message.chat.id, 'Give only 1 argument')
        except Exception as err:
            bot.send_message(message.chat.id, err)


    elif command == 'screen' and len(mes) == 1:
        doc = open(f'{os.getcwd()}\\{system.screenshot()}', 'rb')
        bot.send_document(message.chat.id, doc)
        doc.close()
        os.remove(f'{os.getcwd()}\\{system.screenshot()}')


    elif command == 'ls':
        if len(mes) == 2:
            bot.send_message(message.from_user.id, system.ls(mes[1]), parse_mode='Markdown')
        else:
            bot.send_message(message.from_user.id, system.ls(), parse_mode='Markdown')


    elif command == 'cp':
        bot.send_message(message.from_user.id, system.copy(mes[1], mes[2]))


    elif command == 'mv':
        bot.send_message(message.from_user.id, system.mv(mes[1], mes[2]))


    elif command == 'rm':
        bot.send_message(message.from_user.id, system.rm(mes[1]))

    elif command == 'mkdir':
        try:
            if len(mes) != 2:
                bot.send_message(message.from_user.id, 'Only one argument')
            else:
                os.mkdir(mes[1])
        except Exception as err:
            bot.send_message(message.from_user.id, err)

    elif command == 'start':
        try:
            if len(mes) != 2:
                bot.send_message(message.from_user.id, 'Only one argument')
            else:
                os.system(f'start {mes[1]}')
        except Exception as err:
            bot.send_message(message.from_user.id, err)

    elif command == 'cd':
        try:
            path = mes[1].lower()
            os.chdir(path)
            bot.send_message(message.from_user.id, system.ls(), parse_mode='Markdown')
            print(f'cd to "{path}"')
        except Exception as err:
            bot.send_message(message.from_user.id, err)
            print('cd error')


    elif command == 'getfile':
        try:
            tmp = mes[1].lower()
            doc = open(tmp, 'rb')
            bot.send_document(message.from_user.id, doc)
            doc.close()
            print(f'File "{tmp}" send')
        except Exception as err:
            bot.send_message(message.from_user.id, err)
            print('File not send')


    elif command == 'link':
        if len(mes) == 2:
            if system.link(mes[1]):
                bot.send_message(message.from_user.id, 'Done')
                print(f'Link: {mes[1]}')
            else:
                bot.send_message(message.from_user.id, 'Error')
        else:
            bot.send_message(message.from_user.id, 'Only one argument')


    elif command == 'web':
        if len(mes) > 1:
            tmp = mes[1::]
            tmp2 = ''
            for i in tmp:
                tmp2 += f'{i}+'
            if system.web(tmp2):
                bot.send_message(message.from_user.id, 'Done')
                print(f'Search: {tmp2}')
            else:
                bot.send_message(message.from_user.id, 'Error')
        else:
            bot.send_message(message.from_user.id, 'Give me argument')


    elif command == 'cat':
        if len(mes) == 2:
            try:
                tmp = mes[1::]
                tmp2 = ''
                for i in tmp:
                    tmp2 += f'{i} '
                bot.send_message(message.from_user.id, system.cat(tmp2))
            except Exception as err:
                bot.send_message(message.from_user.id, err)
        else:
            bot.send_message(message.from_user.id, 'Only one argument')


    elif command == 'dpkg':
        try:
            system.unpack(mes[1])
            bot.send_message(message.from_user.id, 'Done')
        except Exception as err:
            bot.send_message(message.from_user.id, err)


    elif command == 'playsound':
        try:
            p = Thread(target=audio.play_sound, args=(f'{mes[1]}',))
            p.start()
            bot.send_message(message.from_user.id, 'Done')
        except:
            bot.reply_to(message, f"Error {err}")


    elif command == 'cmd':
        print(f'[START] CMD')
        try:
            tmp = mes[1::]
            tmp2 = ''
            for i in tmp:
                tmp2 += f'{i} '
            tmp2 = tmp2.strip()
            os.system(tmp2)
        except Exception as err:
            bot.send_message(message.from_user.id, err)
            print(f'[ERROR] {err}')

    elif command == 'cmd2':
        try:
            tmp = mes[1::]
            tmp2 = ''
            for i in tmp:
                tmp2 += f'{i} '
            tmp2 = tmp2.strip()
            bot.send_message(message.from_user.id, subprocess.run(tmp2.split(' '), capture_output=True, text=True, encoding='CP866').stdout)
        except Exception as err:
            bot.send_message(message.from_user.id, err)
            print(f'Cmd error\n\tCommand is "{tmp2}"')

    else:
        if mes[0] not in system.SYSTEM:
            bot.send_message(message.from_user.id, f'Unknown command: {mes[0]}')
            print(mes)
        else:
            bot.send_message(message.from_user.id, f'Done: {mes[0]}')


bot.polling(none_stop=True, interval=0)
