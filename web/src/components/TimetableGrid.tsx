import { useMemo } from 'react'
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
import { type TimeSlot, type Schedule, Days } from '../types'
import { Lock, Unlock, User, School, MapPin, RefreshCw } from 'lucide-react'

interface TimetableGridProps {
  timeSlots: TimeSlot[]
  schedules: Schedule[]
  onToggleLock: (schedule: Schedule) => void
  onMoveSchedule: (scheduleId: string, newTimeSlotId: string) => void
  viewMode: 'classroom' | 'teacher' | 'room'
  loading?: boolean
}

const DAYS = [
  { value: Days.Monday, label: 'Lunes' },
  { value: Days.Tuesday, label: 'Martes' },
  { value: Days.Wednesday, label: 'Miércoles' },
  { value: Days.Thursday, label: 'Jueves' },
  { value: Days.Friday, label: 'Viernes' },
]

export default function TimetableGrid({ 
  timeSlots, 
  schedules, 
  onToggleLock, 
  onMoveSchedule,
  viewMode,
  loading 
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

    const scheduleId = active.id as string
    const newTimeSlotId = over.id as string
    
    onMoveSchedule(scheduleId, newTimeSlotId)
  }

  if (timeSlots.length === 0) {
    return (
      <div className="p-12 text-center border-2 border-dashed border-surface-200 rounded-3xl bg-white">
        <p className="text-surface-500 font-medium italic">No hay bloques horarios definidos para esta sesión.</p>
      </div>
    )
  }

  return (
    <DndContext sensors={sensors} onDragEnd={handleDragEnd}>
      <div className={`overflow-x-auto rounded-[2.5rem] border border-surface-200 bg-white shadow-2xl shadow-surface-200/50 transition-opacity ${loading ? 'opacity-50' : 'opacity-100'}`}>
        <table className="w-full border-collapse">
          <thead>
            <tr className="bg-surface-50/50 border-b border-surface-200">
              <th className="p-6 text-left text-[10px] font-black text-surface-400 uppercase tracking-[0.2em] w-32 border-r border-surface-100 bg-surface-50/30 backdrop-blur-md sticky left-0 z-20">
                Hora
              </th>
              {DAYS.map(day => (
                <th key={day.value} className="p-6 text-center text-[10px] font-black text-surface-400 uppercase tracking-[0.2em] min-w-[220px]">
                  {day.label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="relative min-h-[400px]">
            {loading && (
              <tr className="absolute inset-0 z-50 flex items-center justify-center bg-white/40 backdrop-blur-[2px]">
                <td colSpan={6} className="flex items-center justify-center w-full h-full">
                  <RefreshCw className="animate-spin text-brand-600" size={40} />
                </td>
              </tr>
            )}
            
            {uniqueTimeRanges.map(time => {
              const rowSlots = timeSlots.filter(ts => ts.startTime === time)
              const firstSlot = rowSlots[0]
              const label = firstSlot?.label || time.substring(0, 5)
              const isBreak = firstSlot?.isBreak

              return (
                <tr key={time} className="border-b border-surface-50 last:border-0 group/row">
                  <td className="p-5 text-sm font-medium text-surface-700 bg-surface-50/30 border-r border-surface-100 sticky left-0 z-20 backdrop-blur-md group-hover/row:bg-surface-50 transition-colors">
                    <div className="flex flex-col">
                      <span className="text-brand-600 font-black text-base">{label}</span>
                      <span className="text-[10px] text-surface-400 font-bold uppercase tracking-wider mt-1 opacity-70">
                        {time.substring(0, 5)} - {firstSlot?.endTime.substring(0, 5)}
                      </span>
                    </div>
                  </td>
                  {DAYS.map(day => {
                    const slot = rowSlots.find(ts => ts.dayOfWeek === day.value)
                    const schedule = slot ? schedules.find(s => s.timeSlotId === slot.id) : null

                    if (isBreak) {
                      return (
                        <td key={day.value} className="p-2 bg-surface-50/20">
                          <div className="h-full w-full flex items-center justify-center py-4 bg-surface-50/40 rounded-2xl border border-surface-100/50">
                            <span className="text-[10px] font-black text-surface-300 uppercase tracking-[0.3em]">{day.value === Days.Wednesday ? label : ''}</span>
                          </div>
                        </td>
                      )
                    }

                    return (
                      <td key={day.value} className="p-2 align-top">
                        {slot && <DroppableSlot id={slot.id}>
                          <AnimatePresence mode="popLayout">
                            {schedule ? (
                              <DraggableSchedule 
                                key={schedule.id}
                                schedule={schedule} 
                                onToggleLock={onToggleLock}
                                viewMode={viewMode}
                              />
                            ) : (
                              <div className="h-24 w-full rounded-2xl border-2 border-dashed border-surface-100 flex items-center justify-center text-surface-200 hover:border-brand-200 hover:bg-brand-50/30 transition-all group/empty">
                                <span className="text-[10px] uppercase font-black tracking-[0.2em] group-hover/empty:text-brand-400 transition-colors">Vacío</span>
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
      
      {/* Visual helper for dragging */}
      <DragOverlay dropAnimation={null}>
        {/* We can render a simplified preview of the dragged item here if needed */}
      </DragOverlay>
    </DndContext>
  )
}

function DraggableSchedule({ schedule, onToggleLock, viewMode }: { schedule: Schedule, onToggleLock: (s: Schedule) => void, viewMode: string }) {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: schedule.id,
    disabled: schedule.isLocked
  })

  const style = transform ? {
    transform: `translate3d(${transform.x}px, ${transform.y}px, 0)`,
    zIndex: 100,
    cursor: 'grabbing'
  } : undefined

  return (
    <motion.div
      ref={setNodeRef}
      style={style}
      {...listeners}
      {...attributes}
      layoutId={schedule.id}
      initial={{ scale: 0.9, opacity: 0 }}
      animate={{ scale: 1, opacity: isDragging ? 0.5 : 1 }}
      exit={{ scale: 0.9, opacity: 0 }}
      className={`group relative p-4 rounded-2xl border-2 transition-all h-full min-h-[100px] flex flex-col justify-between select-none ${
        schedule.isLocked 
          ? 'bg-amber-50/80 border-amber-200/50 shadow-sm' 
          : 'bg-white border-brand-100 hover:border-brand-300 hover:shadow-xl hover:shadow-brand-500/10 active:scale-95 cursor-grab'
      }`}
    >
      <div className="flex flex-col gap-2">
        <div className="flex items-start justify-between gap-3">
          <div className="flex flex-col gap-1">
            <span className={`text-xs font-black uppercase tracking-wider leading-tight ${schedule.isLocked ? 'text-amber-800' : 'text-surface-900'}`}>
              {schedule.subject?.name}
            </span>
            {viewMode !== 'classroom' && (
              <div className="flex items-center gap-1.5 text-[10px] font-bold text-brand-600">
                <School size={10} strokeWidth={3} />
                <span>{schedule.classroom?.gradeLevel}º {schedule.classroom?.line}</span>
              </div>
            )}
          </div>
          
          <button 
            onPointerDown={e => e.stopPropagation()} // Prevent drag when clicking lock
            onClick={(e) => { e.stopPropagation(); onToggleLock(schedule) }}
            className={`p-1.5 rounded-lg transition-all ${
              schedule.isLocked 
                ? 'text-amber-500 bg-amber-100/50 hover:bg-amber-100' 
                : 'text-surface-300 bg-surface-50 hover:bg-brand-50 hover:text-brand-600'
            }`}
          >
            {schedule.isLocked ? <Lock size={14} /> : <Unlock size={14} />}
          </button>
        </div>
      </div>
      
      <div className="space-y-1.5 mt-4">
        {viewMode !== 'teacher' && (
          <div className="flex items-center gap-2 text-[10px] font-bold text-surface-500 bg-surface-50 p-1.5 rounded-lg border border-surface-100/50">
            <User size={10} className="shrink-0 text-surface-400" />
            <span className="truncate">{schedule.teacher?.firstName} {schedule.teacher?.lastName}</span>
          </div>
        )}
        
        {viewMode !== 'room' && schedule.room && (
          <div className="flex items-center gap-2 text-[10px] font-bold text-surface-400 px-1.5">
            <MapPin size={10} className="shrink-0" />
            <span className="truncate">{schedule.room.name}</span>
          </div>
        )}
      </div>
      
      {/* Decorative tag for subject color if available */}
      <div 
        className="absolute top-4 left-0 w-1 h-6 rounded-r-full" 
        style={{ backgroundColor: (schedule.subject as any)?.color || '#6366f1' }}
      />
    </motion.div>
  )
}

function DroppableSlot({ id, children }: { id: string, children: React.ReactNode }) {
  const { isOver, setNodeRef } = useDroppable({
    id: id
  })

  return (
    <div 
      ref={setNodeRef} 
      className={`h-full w-full rounded-2xl transition-all duration-300 ${
        isOver ? 'bg-brand-50 ring-2 ring-brand-400 ring-inset scale-[0.98]' : ''
      }`}
    >
      {children}
    </div>
  )
}
