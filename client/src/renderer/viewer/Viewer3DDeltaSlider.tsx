import {
    Slider,
    SliderFilledTrack,
    SliderThumb,
    SliderTrack,
    Tooltip
} from '@chakra-ui/react'
import { useState } from 'react'
import { useProjectStore } from '../store/projectStore'

export default function Viewer3DDeltaSlider() {
    const [showTooltip, setShowTooltip] = useState(false)

    const { project, currentSimulationResultState, setDeltaIndex } =
        useProjectStore()

    if (!project?.Result) {
        throw new Error('Что-то пошло не так')
    }

    const min = -1
    const max = project.Result.Deltas.length - 1

    return (
        <Slider
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
    )
}
