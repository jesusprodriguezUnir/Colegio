import React from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { TimeSlot, Schedule, DayOfWeek } from '../types'
import { Lock, Unlock, User } from 'lucide-react'

interface TimetableGridProps {
  timeSlots: TimeSlot[]
  schedules: Schedule[]
  onToggleLock: (schedule: Schedule) => void
}

const DAYS = [
  { value: DayOfWeek.Monday, label: 'Lunes' },
  { value: DayOfWeek.Tuesday, label: 'Martes' },
  { value: DayOfWeek.Wednesday, label: 'Miércoles' },
  { value: DayOfWeek.Thursday, label: 'Jueves' },
  { value: DayOfWeek.Friday, label: 'Viernes' },
]

export default function TimetableGrid({ timeSlots, schedules, onToggleLock }: TimetableGridProps) {
  // Group slots by time (startTime) to create rows
  const uniqueTimeRanges = Array.from(new Set(timeSlots.map(ts => ts.startTime))).sort()

  if (timeSlots.length === 0) {
    return (
      <div className="p-12 text-center border-2 border-dashed border-surface-200 rounded-3xl bg-white">
        <p className="text-surface-500">No hay bloques horarios definidos para esta sesión.</p>
      </div>
    )
  }

  return (
    <div className="overflow-x-auto rounded-2xl border border-surface-200 bg-white shadow-sm">
      <table className="w-full border-collapse">
        <thead>
          <tr className="bg-surface-50 border-b border-surface-200">
            <th className="p-4 text-left text-xs font-semibold text-surface-500 uppercase tracking-wider w-32 border-r border-surface-200">
              Hora
            </th>
            {DAYS.map(day => (
              <th key={day.value} className="p-4 text-center text-xs font-semibold text-surface-500 uppercase tracking-wider min-w-[200px]">
                {day.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {uniqueTimeRanges.map(time => {
            const rowSlots = timeSlots.filter(ts => ts.startTime === time)
            const label = rowSlots[0]?.label || time.substring(0, 5)
            const isBreak = rowSlots[0]?.isBreak

            return (
              <tr key={time} className="border-b border-surface-100 last:border-0">
                <td className="p-4 text-sm font-medium text-surface-700 bg-surface-50/50 border-r border-surface-200">
                  <div className="flex flex-col">
                    <span className="text-brand-600 font-bold">{label}</span>
                    <span className="text-[10px] text-surface-400 font-normal uppercase tracking-tighter">
                      {time.substring(0, 5)} - {rowSlots[0]?.endTime.substring(0, 5)}
                    </span>
                  </div>
                </td>
                {DAYS.map(day => {
                  const slot = rowSlots.find(ts => ts.dayOfWeek === day.value)
                  const schedule = slot ? schedules.find(s => s.timeSlotId === slot.id) : null

                  if (isBreak) {
                    return (
                      <td key={day.value} className="p-2 bg-surface-50/30 text-center italic text-surface-400 text-xs">
                        {day.value === DayOfWeek.Monday ? label : ''}
                      </td>
                    )
                  }

                  return (
                    <td key={day.value} className="p-2">
                      <AnimatePresence mode="wait">
                        {schedule ? (
                          <motion.div
                            key={schedule.id}
                            initial={{ opacity: 0, y: 10 }}
                            animate={{ opacity: 1, y: 0 }}
                            exit={{ opacity: 0, scale: 0.9 }}
                            className={`group relative p-3 rounded-xl border transition-all h-full min-h-[80px] flex flex-col justify-between ${
                              schedule.isLocked 
                                ? 'bg-amber-50 border-amber-200 shadow-sm' 
                                : 'bg-brand-50 border-brand-100 hover:border-brand-200 hover:shadow-md'
                            }`}
                          >
                            <div className="flex flex-col gap-1">
                              <div className="flex items-start justify-between gap-2">
                                <span className={`text-[11px] font-bold uppercase tracking-tight leading-tight ${schedule.isLocked ? 'text-amber-700' : 'text-brand-700'}`}>
                                  {schedule.subject?.name}
                                </span>
                                <button 
                                  onClick={() => onToggleLock(schedule)}
                                  className={`p-1 rounded-md transition-colors ${
                                    schedule.isLocked ? 'text-amber-500 hover:bg-amber-100' : 'text-brand-300 hover:bg-brand-100 hover:text-brand-600'
                                  }`}
                                >
                                  {schedule.isLocked ? <Lock size={12} /> : <Unlock size={12} />}
                                </button>
                              </div>
                            </div>
                            
                            <div className="flex items-center gap-1.5 text-[10px] text-surface-500 mt-2 border-t border-black/5 pt-1.5">
                              <User size={10} className="shrink-0" />
                              <span className="truncate">{schedule.teacher?.firstName} {schedule.teacher?.lastName}</span>
                            </div>
                          </motion.div>
                        ) : (
                          <div className="h-20 rounded-xl border border-dashed border-surface-200 flex items-center justify-center text-surface-300">
                            <span className="text-[10px] uppercase font-medium tracking-widest">Libre</span>
                          </div>
                        )}
                      </AnimatePresence>
                    </td>
                  )
                })}
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}
