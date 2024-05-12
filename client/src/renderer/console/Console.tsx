import {
    Box,
    Button,
    Card,
    Code,
    Flex,
    IconButton,
    Text,
    Tooltip,
    useTheme
} from '@chakra-ui/react'
import { IconCaretDown, IconCaretUp, IconTrashX } from '@tabler/icons-react'
import { useEffect, useRef } from 'react'
import useConsoleStore from '../store/consoleStore'

export default function Console() {
    const {
        isOpen,
        setIsOpen,
        consoleOutput,
        clearConsoleOutput,
        addLineToConsoleOutput
    } = useConsoleStore()

    const theme = useTheme()
    const bg = theme.__cssMap['colors.chakra-body-bg'].value

    const divRef = useRef<HTMLDivElement>(null)

    useEffect(() => {
        if (divRef.current) divRef.current.scrollIntoView()
    }, [consoleOutput])

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
                    `Процесс завершен с кодом ${data as number}`,
                    true
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
        <Flex position='relative' direction='column' h={isOpen ? '33vh' : ''}>
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
                <Card h='100%' borderTopRadius={0} overflowY='scroll'>
                    <Code padding={4} whiteSpace='pre' backgroundColor={bg}>
                        {consoleOutput.length === 0
                            ? 'Пусто...'
                            : consoleOutput.map((value, index) => (
                                  <Text key={index}>{value}</Text>
                              ))}
                        <Box ref={divRef}></Box>
                    </Code>
                </Card>
            )}

            <Tooltip label='Очистить консоль'>
                <IconButton
                    onClick={clearConsoleOutput}
                    aria-label='Очистить консоль'
                    position='absolute'
                    top={7}
                    right={5}
                >
                    <IconTrashX />
                </IconButton>
            </Tooltip>
        </Flex>
    )
}
