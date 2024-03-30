from tkinter import *
import time

deanon = '''
██╗  ░██████╗███████╗███████╗  ██╗░░░██╗░█████╗░██╗░░░██╗
██║  ██╔════╝██╔════╝██╔════╝  ╚██╗░██╔╝██╔══██╗██║░░░██║
██║  ╚█████╗░█████╗░░█████╗░░  ░╚████╔╝░██║░░██║██║░░░██║
██║  ░╚═══██╗██╔══╝░░██╔══╝░░  ░░╚██╔╝░░██║░░██║██║░░░██║
██║  ██████╔╝███████╗███████╗  ░░░██║░░░╚█████╔╝╚██████╔╝
╚═╝  ╚═════╝░╚══════╝╚══════╝  ░░░╚═╝░░░░╚════╝░░╚═════╝░'''

def oops():
    root = Tk()

    text = Text(width=65, height=10, bg='black', fg='red')
    root.title = 'Oops'
    text.pack()
    text.insert(1.0, deanon)

    text.tag_add('title', 1.0, '1.end')
    text.tag_config('title', justify=CENTER,
                    font=("Verdana", 24, 'bold'),)

    root.mainloop()