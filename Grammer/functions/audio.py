import pyaudio
import wave
import functions.script as script
import pygame
import time

def device_list():
    p = pyaudio.PyAudio()
    listTmp = []
    out = ''
    for i in range(p.get_device_count()):
        tmp = f'{i}. ' + p.get_device_info_by_index(i)['name']
        listTmp.append(tmp)
    for i in listTmp:
        out += i + '\n'
    return out

def streaming():
    p = pyaudio.PyAudio()

    if script.device != None:
        stream = p.open(format=script.form,
                        channels=script.channels,
                        rate=script.rate,
                        input=True,
                        frames_per_buffer=script.chunk,
                        input_device_index=script.device)
    else:
        stream = p.open(form=script.form,
                        channels=script.channels,
                        rate=script.rate,
                        input=True,
                        frames_per_buffer=script.chunk)

    frames = []

    for i in range(0, int(script.rate / script.chunk * script.record_seconds)):
        data = stream.read(script.chunk)
        frames.append(data)

    stream.stop_stream()
    stream.close()
    p.terminate()

    wf = wave.open(script.WAVE_OUTPUT_FILENAME, 'wb')
    wf.setnchannels(script.channels)
    wf.setsampwidth(p.get_sample_size(script.form))
    wf.setframerate(script.rate)
    wf.writeframes(b''.join(frames))
    wf.close()

def savepreset():
    with open(f'{script.work_directory}\\config.txt', 'w') as f:
        f.write(f'{script.chunk} {script.channels} {script.rate} {script.record_seconds} {script.device}')

def play_sound(file):
    pygame.init()
    pygame.mixer.Sound(file).play()
    time.sleep(600)
    pygame.quit()
