import { useMemo, useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import {
  DndContext,
  DragOverlay,
  useDraggable,
  useDroppable,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent
} from '@dnd-kit/core'
import { type TimeSlot, type Schedule, type ConflictInfo, Days } from '../types'
import { Lock, Unlock, User, School, MapPin, RefreshCw, Calendar, Clock } from 'lucide-react'

interface TimetableGridProps {
  timeSlots: TimeSlot[]
  schedules: Schedule[]
  onToggleLock: (schedule: Schedule) => void
  onMoveSchedule: (scheduleId: string, newTimeSlotId: string) => void
  viewMode: 'classroom' | 'teacher' | 'room'
  loading?: boolean
  conflicts?: ConflictInfo[]
}

const DAYS = [
  { value: Days.Monday, label: 'Lunes', short: 'LUN' },
  { value: Days.Tuesday, label: 'Martes', short: 'MAR' },
  { value: Days.Wednesday, label: 'Miércoles', short: 'MIÉ' },
  { value: Days.Thursday, label: 'Jueves', short: 'JUE' },
  { value: Days.Friday, label: 'Viernes', short: 'VIE' },
]

export default function TimetableGrid({
  timeSlots,
  schedules,
  onToggleLock,
  onMoveSchedule,
  viewMode,
  loading,
  conflicts = [],
}: TimetableGridProps) {
  const sensors = useSensors(useSensor(PointerSensor, {
    activationConstraint: { distance: 8 }
  }))

  const uniqueTimeRanges = useMemo(() =>
    Array.from(new Set(timeSlots.map(ts => ts.startTime))).sort(),
    [timeSlots]
  )

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event
    if (!over) return
    onMoveSchedule(active.id as string, over.id as string)
  }

  if (timeSlots.length === 0) {
    return (
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="p-16 text-center border-2 border-dashed border-surface-200 rounded-[3rem] bg-white/50 backdrop-blur-xl shadow-inner"
      >
        <div className="bg-surface-100 w-16 h-16 rounded-2xl flex items-center justify-center mx-auto mb-6">
          <Calendar className="text-surface-400" size={32} />
        </div>
        <h3 className="text-xl font-bold text-surface-900 mb-2">No hay horarios definidos</h3>
        <p className="text-surface-500 max-w-xs mx-auto">Selecciona un aula y un tipo de sesión para visualizar o generar el horario.</p>
      </motion.div>
    )
  }

  return (
    <DndContext sensors={sensors} onDragEnd={handleDragEnd}>
      <div className={`relative overflow-hidden rounded-[2.5rem] border border-white/40 bg-white/40 backdrop-blur-2xl shadow-2xl shadow-surface-200/50 transition-all duration-500 ${loading ? 'scale-[0.99] opacity-70' : 'scale-100 opacity-100'}`}>
        <div className="overflow-x-auto">
          <table className="w-full border-separate border-spacing-0">
            <thead>
              <tr>
                <th className="p-8 text-left border-b border-surface-100 bg-white/60 sticky left-0 z-30 backdrop-blur-xl min-w-[140px]">
                  <div className="flex items-center gap-2 text-surface-400">
                    <Clock size={16} />
                    <span className="text-[10px] font-black uppercase tracking-[0.2em]">Horario</span>
                  </div>
                </th>
                {DAYS.map(day => (
                  <th key={day.value} className="p-8 text-center border-b border-surface-100 bg-white/40 backdrop-blur-xl min-w-[240px]">
                    <div className="flex flex-col items-center gap-1">
                      <span className="text-[10px] font-black text-brand-600 uppercase tracking-[0.3em]">{day.short}</span>
                      <span className="text-lg font-bold text-surface-900">{day.label}</span>
                    </div>
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="relative">
              <AnimatePresence>
                {loading && (
                  <motion.tr
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    className="absolute inset-0 z-50 flex items-center justify-center bg-white/20 backdrop-blur-md"
                  >
                    <td colSpan={6} className="flex items-center justify-center w-full h-full min-h-[400px]">
                      <div className="flex flex-col items-center gap-4">
                        <div className="relative">
                          <div className="w-16 h-16 rounded-full border-4 border-brand-100 border-t-brand-600 animate-spin" />
                          <RefreshCw className="absolute inset-0 m-auto text-brand-600 animate-pulse" size={24} />
                        </div>
                        <span className="text-sm font-bold text-brand-900 animate-pulse">Optimizando Horarios...</span>
                      </div>
                    </td>
                  </motion.tr>
                )}
              </AnimatePresence>

              {uniqueTimeRanges.map((time, idx) => {
                const rowSlots = timeSlots.filter(ts => ts.startTime === time)
                const firstSlot = rowSlots[0]
                const label = firstSlot?.label || `${idx + 1}ª Hora`
                const isBreak = firstSlot?.isBreak

                return (
                  <tr key={time} className="group/row">
                    <td className="p-6 text-sm font-medium border-r border-surface-50 bg-white/60 sticky left-0 z-20 backdrop-blur-xl group-hover/row:bg-brand-50/30 transition-colors">
                      <div className="flex flex-col">
                        <span className="text-brand-600 font-black text-lg leading-tight">{label}</span>
                        <div className="flex items-center gap-1.5 mt-2">
                          <span className="px-2 py-0.5 rounded-md bg-surface-100 text-[10px] font-black text-surface-500 uppercase">
                            {time.substring(0, 5)}
                          </span>
                          <div className="w-1 h-[1px] bg-surface-200" />
                          <span className="px-2 py-0.5 rounded-md bg-surface-100 text-[10px] font-black text-surface-500 uppercase">
                            {firstSlot?.endTime.substring(0, 5)}
                          </span>
                        </div>
                      </div>
                    </td>
                    {DAYS.map(day => {
                      const slot = rowSlots.find(ts => ts.dayOfWeek === day.value)
                      const schedule = slot ? schedules.find(s => s.timeSlotId === slot.id) : null

                      if (isBreak) {
                        return (
                          <td key={day.value} className="p-3 bg-surface-50/30">
                            <div className="h-full w-full flex items-center justify-center py-6 bg-white/40 rounded-3xl border border-surface-200/50 shadow-inner">
                              <span className="text-[10px] font-black text-surface-300 uppercase tracking-[0.4em]">
                                {day.value === Days.Wednesday ? label : ''}
                              </span>
                            </div>
                          </td>
                        )
                      }

                      return (
                        <td key={day.value} className="p-3 align-top">
                          {slot && <DroppableSlot id={slot.id}>
                            <AnimatePresence mode="popLayout">
                              {schedule ? (
                                <DraggableSchedule
                                  key={schedule.id}
                                  schedule={schedule}
                                  onToggleLock={onToggleLock}
                                  viewMode={viewMode}
                                  conflicts={conflicts}
                                />
                              ) : (
                                <div className="h-32 w-full rounded-[2rem] border-2 border-dashed border-surface-100 flex items-center justify-center text-surface-300 hover:border-brand-200 hover:bg-brand-50/50 hover:text-brand-400 transition-all duration-300 group/empty">
                                  <div className="flex flex-col items-center gap-2 opacity-0 group-hover/empty:opacity-100 transition-opacity">
                                    <div className="p-2 rounded-xl bg-brand-100/50">
                                      <Calendar size={16} />
                                    </div>
                                    <span className="text-[10px] uppercase font-black tracking-[0.2em]">Asignar</span>
                                  </div>
                                </div>
                              )}
                            </AnimatePresence>
                          </DroppableSlot>}
                        </td>
                      )
                    })}
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      </div>

      <DragOverlay dropAnimation={null} />
    </DndContext>
  )
}

function DraggableSchedule({
  schedule,
  onToggleLock,
  viewMode,
  conflicts,
}: {
  schedule: Schedule
  onToggleLock: (s: Schedule) => void
  viewMode: string
  conflicts: ConflictInfo[]
}) {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: schedule.id,
    disabled: schedule.isLocked
  })
  const [isHovered, setIsHovered] = useState(false)

  const style = transform ? {
    transform: `translate3d(${transform.x}px, ${transform.y}px, 0)`,
    zIndex: 100,
    cursor: 'grabbing'
  } : undefined

  const subjectColor = (schedule.subject as any)?.color || '#6366f1'

  const hasError = conflicts.some(c =>
    c.severity === 'Error' && (c.timeSlotId === schedule.timeSlotId || c.teacherId === schedule.teacherId)
  )
  const hasWarning = !hasError && conflicts.some(c =>
    c.severity === 'Warning' && (c.timeSlotId === schedule.timeSlotId || c.teacherId === schedule.teacherId)
  )

  const conflictClass = hasError
    ? 'ring-2 ring-red-500 animate-pulse'
    : hasWarning
    ? 'ring-2 ring-yellow-400'
    : ''

  return (
    <motion.div
      ref={setNodeRef}
      style={style}
      {...listeners}
      {...attributes}
      layoutId={schedule.id}
      initial={{ scale: 0.9, opacity: 0 }}
      animate={{
        scale: 1,
        opacity: isDragging ? 0.5 : 1,
        y: isDragging ? -10 : 0
      }}
      exit={{ scale: 0.9, opacity: 0 }}
      whileHover={{ y: -4, transition: { duration: 0.2 } }}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      className={`group relative p-5 rounded-[2rem] border-2 transition-all h-full min-h-[120px] flex flex-col justify-between shadow-sm overflow-visible ${conflictClass} ${
        schedule.isLocked
          ? 'bg-amber-50/50 border-amber-200/30'
          : 'bg-white border-surface-100 hover:border-brand-200 hover:shadow-2xl hover:shadow-brand-500/10 cursor-grab active:cursor-grabbing'
      }`}
    >
      {/* Lateral color bar (Peñalara style) */}
      <div
        className="absolute top-0 left-0 w-1 h-full rounded-l-[2rem]"
        style={{ backgroundColor: subjectColor }}
      />

      {/* Subtle background tint */}
      <div
        className="absolute inset-0 rounded-[2rem] pointer-events-none"
        style={{ backgroundColor: subjectColor + '18' }}
      />

      <div className="relative flex flex-col gap-3">
        <div className="flex items-start justify-between gap-3">
          <div className="flex flex-col gap-1.5">
            <span
              className="text-sm font-black uppercase tracking-tight leading-tight"
              style={{ color: subjectColor }}
            >
              {schedule.subject?.name}
            </span>
            {viewMode !== 'classroom' && (
              <div className="flex items-center gap-1.5 text-[10px] font-black text-brand-600 bg-brand-50/50 px-2 py-0.5 rounded-lg w-fit">
                <School size={10} strokeWidth={3} />
                <span>{schedule.classroom?.gradeLevel}º {schedule.classroom?.line}</span>
              </div>
            )}
          </div>

          <button
            onPointerDown={e => e.stopPropagation()}
            onClick={(e) => { e.stopPropagation(); onToggleLock(schedule) }}
            className={`p-2 rounded-xl transition-all ${
              schedule.isLocked
                ? 'text-amber-600 bg-amber-100/50 hover:bg-amber-200/50'
                : 'text-surface-300 bg-surface-50 hover:bg-brand-50 hover:text-brand-600'
            }`}
          >
            {schedule.isLocked ? <Lock size={14} strokeWidth={3} /> : <Unlock size={14} strokeWidth={3} />}
          </button>
        </div>
      </div>

      <div className="relative space-y-2 mt-4">
        {viewMode !== 'teacher' && (
          <div className="flex items-center gap-2.5 text-[10px] font-bold text-surface-600 bg-surface-50/50 p-2 rounded-xl border border-surface-100/50 backdrop-blur-sm group-hover:bg-white transition-colors">
            <div className="w-5 h-5 rounded-lg bg-white flex items-center justify-center shadow-sm">
              <User size={10} className="text-brand-500" />
            </div>
            <span className="truncate">{schedule.teacher?.firstName} {schedule.teacher?.lastName}</span>
          </div>
        )}

        {viewMode !== 'room' && schedule.room && (
          <div className="flex items-center gap-2.5 text-[10px] font-bold text-surface-400 px-2">
            <MapPin size={12} className="shrink-0 text-surface-300" />
            <span className="truncate uppercase tracking-wider">{schedule.room.name}</span>
          </div>
        )}
      </div>

      {/* Tooltip */}
      <AnimatePresence>
        {isHovered && !isDragging && (
          <motion.div
            initial={{ opacity: 0, y: 4, scale: 0.95 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: 4, scale: 0.95 }}
            transition={{ duration: 0.15 }}
            className="absolute bottom-full left-1/2 -translate-x-1/2 mb-3 z-50 w-56 bg-surface-900 text-white rounded-2xl p-4 shadow-2xl pointer-events-none"
          >
            <div className="space-y-2 text-xs">
              <div className="font-black text-sm" style={{ color: subjectColor }}>
                {schedule.subject?.name}
              </div>
              <div className="flex items-center gap-2 text-surface-300">
                <User size={11} />
                <span>{schedule.teacher?.firstName} {schedule.teacher?.lastName}</span>
              </div>
              {schedule.classroom && (
                <div className="flex items-center gap-2 text-surface-300">
                  <School size={11} />
                  <span>{schedule.classroom.gradeLevel}º {schedule.classroom.line}</span>
                </div>
              )}
              {schedule.room && (
                <div className="flex items-center gap-2 text-surface-300">
                  <MapPin size={11} />
                  <span>{schedule.room.name}</span>
                </div>
              )}
              {schedule.isLocked && (
                <div className="flex items-center gap-2 text-amber-400">
                  <Lock size={11} />
                  <span>Sesión bloqueada</span>
                </div>
              )}
              {hasError && (
                <div className="flex items-center gap-2 text-red-400">
                  <span>⚠ Conflicto detectado</span>
                </div>
              )}
            </div>
            {/* Arrow */}
            <div className="absolute top-full left-1/2 -translate-x-1/2 w-3 h-3 bg-surface-900 rotate-45 -mt-1.5" />
          </motion.div>
        )}
      </AnimatePresence>

      {/* Decorative glow */}
      {!schedule.isLocked && (
        <div
          className="absolute -bottom-8 -right-8 w-16 h-16 rounded-full blur-3xl opacity-0 group-hover:opacity-20 transition-opacity"
          style={{ backgroundColor: subjectColor }}
        />
      )}
    </motion.div>
  )
}

function DroppableSlot({ id, children }: { id: string, children: React.ReactNode }) {
  const { isOver, setNodeRef } = useDroppable({ id })

  return (
    <div
      ref={setNodeRef}
      className={`h-full w-full rounded-[2.5rem] transition-all duration-500 relative ${isOver ? 'bg-brand-50/50 scale-[0.98]' : ''}`}
    >
      {isOver && (
        <motion.div
          layoutId="drop-indicator"
          className="absolute inset-0 rounded-[2.5rem] ring-4 ring-brand-400/30 ring-inset z-10"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
        />
      )}
      {children}
    </div>
  )
}
