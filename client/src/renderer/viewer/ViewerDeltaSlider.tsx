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

export default function ViewerDeltaSlider() {
    const [showTooltip, setShowTooltip] = useState(false)

    const { project, simulationDeltaIndex, simulationState, setDeltaIndex } =
        useProjectStore()

    if (!project?.Result) {
        throw new Error('Что-то пошло не так')
    }

    const min = -1
    const max = project.Result.Deltas.length - 1

    return (
        <Flex direction='row' gap={4}>
            <Button
                isDisabled={simulationDeltaIndex === min}
                size='xs'
                onClick={() => {
                    if (simulationDeltaIndex > min) {
                        setDeltaIndex(simulationDeltaIndex - 1)
                    }
                }}
            >
                <IconChevronLeft />
            </Button>

            <Slider
                flexGrow={1}
                id='slider'
                value={simulationDeltaIndex}
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
                    label={`${simulationState.Time.replace('T', ' ')}`}
                >
                    <SliderThumb />
                </Tooltip>
            </Slider>

            <Button
                isDisabled={simulationDeltaIndex === max}
                size='xs'
                onClick={() => {
                    if (simulationDeltaIndex < max) {
                        setDeltaIndex(simulationDeltaIndex + 1)
                    }
                }}
            >
                <IconChevronRight />
            </Button>
        </Flex>
    )
}
