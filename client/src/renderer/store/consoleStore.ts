import { create } from 'zustand'

interface State {
    consoleOutput: string[]
    isOpen: boolean
    addLineToConsoleOutput: (value: string) => void
    setIsOpen: (value: boolean) => void
}

const useConsoleStore = create<State>((set, state) => ({
    consoleOutput: [],
    isOpen: false,
    addLineToConsoleOutput: (value: string) => {
        state().consoleOutput.push(value)
    },
    setIsOpen: (value: boolean) => set({ isOpen: value })
}))

export default useConsoleStore
