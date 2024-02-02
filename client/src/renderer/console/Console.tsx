import { Button, Card, CardBody, Flex, Text } from '@chakra-ui/react'
import { IconCaretDown, IconCaretUp } from '@tabler/icons-react'
import { useState } from 'react'
import useAppStore from '../store'

export default function Console() {
    const { consoleOutput, setConsoleOutput } = useAppStore()
    const [isOpen, setIsOpen] = useState(true)

    window.electron.ipcRenderer.on('shell-reply', (data) => {
        setConsoleOutput(consoleOutput + '\n' + data)
        console.log(1)
    })

    return (
        <Flex direction='column' gap={1}>
            <Button size='xs' onClick={() => setIsOpen(!isOpen)}>
                {isOpen ? (
                    <>
                        <IconCaretDown /> Скрыть
                    </>
                ) : (
                    <>
                        <IconCaretUp /> Показать
                    </>
                )}{' '}
                вывод консоли
            </Button>
            {isOpen && (
                <Card h='33vh'>
                    <CardBody fontFamily='monospace' overflowY='scroll'>
                        {consoleOutput === ''
                            ? 'Пусто...'
                            : consoleOutput
                                  .split('\n')
                                  .map((value, index) => (
                                      <Text key={index}>{value}</Text>
                                  ))}
                    </CardBody>
                </Card>
            )}
        </Flex>
    )
}
