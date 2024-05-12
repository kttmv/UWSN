import {
    Flex,
    Heading,
    IconButton,
    Popover,
    PopoverArrow,
    PopoverBody,
    PopoverCloseButton,
    PopoverContent,
    PopoverHeader,
    PopoverTrigger,
    Text,
    Tooltip
} from '@chakra-ui/react'
import {
    IconArrowsMaximize,
    IconArrowsMinimize,
    IconSettings
} from '@tabler/icons-react'
import useViewerStore from '../store/viewerStore'
import ViewerScaleSlider from './ViewerScaleSlider'

export default function ViewerSettings() {
    const { isFullscreen, setIsFullscreen } = useViewerStore()

    return (
        <Flex
            direction='column'
            gap={1}
            position='absolute'
            top='5px'
            left='5px'
        >
            <Popover>
                <PopoverTrigger>
                    <IconButton aria-label='Открыть меню настроек 3D просмотра'>
                        <Tooltip label='Настройки 3D просмотра'>
                            <IconSettings />
                        </Tooltip>
                    </IconButton>
                </PopoverTrigger>
                <PopoverContent padding='10px'>
                    <PopoverArrow />
                    <PopoverCloseButton />
                    <PopoverHeader>
                        <Heading size='md'> Настройки </Heading>
                    </PopoverHeader>
                    <PopoverBody>
                        <Text>Масштаб</Text>
                        <ViewerScaleSlider />
                    </PopoverBody>
                </PopoverContent>
            </Popover>
            <Tooltip label='Включить/выключить полноэкранный режим'>
                <IconButton
                    onClick={() => setIsFullscreen(!isFullscreen)}
                    aria-label='Включить/выключить полноэкранный режим'
                >
                    {isFullscreen ? (
                        <IconArrowsMinimize />
                    ) : (
                        <IconArrowsMaximize />
                    )}
                </IconButton>
            </Tooltip>
        </Flex>
    )
}
