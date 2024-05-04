import { ChildProcessWithoutNullStreams, spawn } from 'child_process'
import {
    app,
    dialog,
    ipcMain,
    IpcMainEvent,
    OpenDialogOptions,
    SaveDialogOptions
} from 'electron'
import fs from 'fs'
import os from 'os'

// -----------------------------------------------------------------------------
// Эти функции нужны для того, чтобы строго соблюдалась типизация каналов.

// Для IPC использовать только эти функции, не вызывать ipcMain.on и
// event.reply вручную!

function on(
    channel: Channels,
    listener: (event: IpcMainEvent, ...args: unknown[]) => void
) {
    ipcMain.on(channel, listener)
}

function reply(event: IpcMainEvent, channel: ReplyChannels, arg: unknown) {
    let str = JSON.stringify(arg)

    if (str.length > 100) {
        str = str.substring(0, 100) + '...'
    }

    console.log(`IPC REPLY (${channel}):\n${str}`)
    event.reply(channel, arg)
}
// -----------------------------------------------------------------------------

function runShell(
    event: IpcMainEvent,
    args: string
): ChildProcessWithoutNullStreams {
    const isWindows = os.platform() === 'win32'
    const isLinux = os.platform() === 'linux'

    const path = app.isPackaged
        ? isWindows
            ? 'simulator\\UWSN.exe'
            : 'simulator/UWSN'
        : isWindows
          ? '..\\UWSN\\bin\\Debug\\net7.0\\UWSN.exe'
          : 'dotnet run --project ../UWSN --'

    const child = spawn(`${path} ${args}`, [], {
        shell: true
    })

    child.stderr.on('data', (data) => {
        console.error(`stderr: ${data}`)
        reply(event, 'run-shell-reply', data.toString())
    })

    child.on('close', (code) => {
        console.log(`child process exited with code ${code}`)
        reply(event, 'run-shell-close', code)
    })

    return child
}

on('run-shell', (event, args) => {
    console.log('\nRUN SIMULATOR')
    console.log('ARGS: ', args)

    const child = runShell(event, args as string)

    child.stdout.on('data', (data) => {
        console.log(`stdout: ${data}`)
        reply(event, 'run-shell-reply', data.toString())
    })
})

const SEND_REPLIES_EVERY_NTH_EVENT = 10_000

on('run-shell-simulation', (event, args) => {
    console.log('\nRUN SIMULATOR (NO STDOUT)')
    console.log('ARGS: ', args)

    const child = runShell(event, args as string)

    child.stdout.on('data', (data) => {
        console.log(`stdout: ${data}`)

        const line = data.toString() as string

        const matchEvent = line.match(/\[(.+)\] Событие №(\d+)/)
        if (matchEvent) {
            const time = matchEvent[1]
            const number = parseInt(matchEvent[2])
            if (number % SEND_REPLIES_EVERY_NTH_EVENT === 0) {
                reply(
                    event,
                    'run-shell-reply',
                    `Обработано событий: ${number}. Время симуляции: ${time}`
                )
            }
        }

        const matchStop = line.match(/\[(.+)\] Симуляция остановлена./)
        if (matchStop) {
            const time = matchStop[1]
            reply(
                event,
                'run-shell-reply',
                `Симуляция остановлена. Конечное время симуляции: ${time}`
            )
        }

        if (line.includes('Был достигнут лимит событий')) {
            reply(event, 'run-shell-reply', line)
        }
    })
})

on('read-file', (event, arg) => {
    console.log(arg)

    const path = arg as string

    if (!path) {
        reply(event, 'read-file-reply', {
            error: 'Путь к файлу пуст.'
        } as ReadFileReply)

        return
    }

    fs.stat(path, function (error) {
        if (error) {
            if (error.code === 'ENOENT') {
                reply(event, 'read-file-reply', {
                    error: 'Указанный файл не существует.'
                } as ReadFileReply)
            } else {
                reply(event, 'read-file-reply', {
                    error: `Ошибка при чтении файла: ${error.message}`
                } as ReadFileReply)
            }

            return
        }
    })

    fs.readFile(path, 'utf8', (error, data) => {
        if (error) {
            reply(event, 'read-file-reply', {
                error: `Ошибка при чтении файла: ${error.message}`
            } as ReadFileReply)

            return
        }

        reply(event, 'read-file-reply', {
            content: data
        } as ReadFileReply)
    })
})

on('open-file-dialog', (event, arg) => {
    console.log(arg)

    const data = arg as OpenDialogOptions

    dialog.showOpenDialog(data).then((result) => {
        reply(event, 'open-file-dialog-reply', result)
    })
})

on('save-file-dialog', (event, arg) => {
    console.log(arg)

    const data = arg as SaveDialogOptions

    dialog.showSaveDialog(data).then((result) => {
        reply(event, 'save-file-dialog-reply', result)
    })
})

on('write-file', (event, arg) => {
    console.log(arg)

    const data = arg as WriteFileData

    fs.writeFile(data.path, data.content, (error) => {
        if (error) {
            reply(
                event,
                'write-file-reply',
                `Ошибка при записи файла: ${error.message}`
            )
        } else {
            reply(event, 'write-file-reply', {})
        }
    })
})
