import {
    Button,
    Flex,
    Slider,
    SliderFilledTrack,
    SliderThumb,
    SliderTrack,
    Tooltip
} from '@chakra-ui/react'
import { IconChevronLeft, IconChevronRight } from '@tabler/icons-react'
import { useState } from 'react'
import { useProjectStore } from '../store/projectStore'

export default function Viewer3DDeltaSlider() {
    const [showTooltip, setShowTooltip] = useState(false)

    const {
        project,
        currentDeltaIndex,
        currentSimulationResultState,
        setDeltaIndex
    } = useProjectStore()

    if (!project?.Result) {
        throw new Error('Что-то пошло не так')
    }

    const min = -1
    const max = project.Result.Deltas.length - 1

    return (
        <Flex direction='row' gap={4}>
            <Button
                isDisabled={currentDeltaIndex === min}
                size='xs'
                onClick={() => {
                    if (currentDeltaIndex > min) {
                        setDeltaIndex(currentDeltaIndex - 1)
                    }
                }}
            >
                <IconChevronLeft />
            </Button>

            <Slider
                flexGrow={1}
                id='slider'
                defaultValue={-1}
                colorScheme='teal'
                onChange={(v) => setDeltaIndex(v)}
                onMouseEnter={() => setShowTooltip(true)}
                onMouseLeave={() => setShowTooltip(false)}
                min={min}
                max={max}
            >
                <SliderTrack>
                    <SliderFilledTrack />
                </SliderTrack>

                <Tooltip
                    hasArrow
                    bg='teal.500'
                    color='white'
                    placement='top'
                    isOpen={showTooltip}
                    label={`${currentSimulationResultState.Time.replace('T', ' ')}`}
                >
                    <SliderThumb />
                </Tooltip>
            </Slider>

            <Button
                isDisabled={currentDeltaIndex === max}
                size='xs'
                onClick={() => {
                    if (currentDeltaIndex < max) {
                        setDeltaIndex(currentDeltaIndex + 1)
                    }
                }}
            >
                <IconChevronRight />
            </Button>
        </Flex>
    )
}
