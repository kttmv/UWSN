import { create } from 'zustand'

interface State {
    consoleOutput: string[]
    isOpen: boolean
    addLineToConsoleOutput: (value: string) => void
    setIsOpen: (value: boolean) => void
}

const useConsoleStore = create<State>((set) => ({
    consoleOutput: [],
    isOpen: false,
    addLineToConsoleOutput: (value: string) =>
        set((state) => ({ consoleOutput: [...state.consoleOutput, value] })),
    setIsOpen: (value: boolean) => set({ isOpen: value })
}))

export default useConsoleStore
