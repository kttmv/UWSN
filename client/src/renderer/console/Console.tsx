import { Button, Card, CardBody, Flex, Text } from '@chakra-ui/react'
import { IconCaretDown, IconCaretUp } from '@tabler/icons-react'
import { useEffect, useState } from 'react'
import useConsoleStore from '../store/consoleStore'

export default function Console() {
    const { consoleOutput, addLineToConsoleOutput } = useConsoleStore()
    const [isOpen, setIsOpen] = useState(false)

    useEffect(() => {
        const removeListener = window.electronAPI.ipcRenderer.on(
            'run-simulator-reply',
            (data) => {
                addLineToConsoleOutput(data as string)
            }
        )

        return removeListener
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
