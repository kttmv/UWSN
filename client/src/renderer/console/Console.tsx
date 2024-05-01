import { Button, Card, CardBody, Flex, Text } from '@chakra-ui/react'
import { IconCaretDown, IconCaretUp } from '@tabler/icons-react'
import { useEffect } from 'react'
import useConsoleStore from '../store/consoleStore'

export default function Console() {
    const { isOpen, setIsOpen, consoleOutput, addLineToConsoleOutput } =
        useConsoleStore()

    useEffect(() => {
        const removeReplyListener = window.electronAPI.ipcRenderer.on(
            'run-shell-reply',
            (data) => {
                addLineToConsoleOutput(data as string)
            }
        )

        const removeCloseListener = window.electronAPI.ipcRenderer.on(
            'run-shell-close',
            (data) => {
                addLineToConsoleOutput(
                    `Процесс завершен с кодом ${data as number}`
                )
            }
        )

        const remove = () => {
            removeReplyListener()
            removeCloseListener()
        }

        return remove
    }, [])

    return (
        <Flex direction='column' h={isOpen ? '33vh' : ''}>
            <Button
                borderBottomRadius={0}
                size='xs'
                onClick={() => setIsOpen(!isOpen)}
            >
                {isOpen ? (
                    <>
                        <IconCaretDown /> Скрыть вывод консоли
                    </>
                ) : (
                    <>
                        <IconCaretUp /> Показать вывод консоли
                    </>
                )}
            </Button>
            {isOpen && (
                <Card h='100%' overflowY='scroll' borderTopRadius={0}>
                    <CardBody fontFamily='monospace'>
                        {consoleOutput.length === 0
                            ? 'Пусто...'
                            : consoleOutput.map((value, index) => (
                                  <Text key={index}>{value}</Text>
                              ))}
                    </CardBody>
                </Card>
            )}
        </Flex>
    )
}
