import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Plus, 
  Search, 
  Edit2, 
  Trash2, 
  DoorOpen, 
  School,
  X,
  UserSquare2,
  Users,
  GraduationCap,
  Calendar,
  Layers
} from 'lucide-react'
import { classroomsApi } from '../services/api'
import type { Classroom, Student } from '../types'

export default function Classrooms() {
  const [classrooms, setClassrooms] = useState<Classroom[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<Classroom | null>(null)
  const [selectedClassroom, setSelectedClassroom] = useState<Classroom | null>(null)
  const [form, setForm] = useState({ gradeLevel: 1, line: 'A', schoolId: '', tutorId: '' })
  const [searchTerm, setSearchTerm] = useState('')

  useEffect(() => {
    loadClassrooms()
  }, [])

  const loadClassrooms = async () => {
    try {
      const res = await classroomsApi.getAll()
      setClassrooms(res.data)
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editing) {
        await classroomsApi.update(editing.id, form)
      } else {
        await classroomsApi.create(form)
      }
      resetForm()
      loadClassrooms()
    } catch (e) {
      console.error(e)
    }
  }

  const resetForm = () => {
    setForm({ gradeLevel: 1, line: 'A', schoolId: '', tutorId: '' })
    setShowForm(false)
    setEditing(null)
  }

  const handleDelete = async (id: string, e: React.MouseEvent) => {
    e.stopPropagation()
    if (confirm('¿Estás seguro de que deseas eliminar esta aula?')) {
      try {
        await classroomsApi.delete(id)
        loadClassrooms()
      } catch (e) {
        console.error(e)
      }
    }
  }

  const handleEdit = (classroom: Classroom, e: React.MouseEvent) => {
    e.stopPropagation()
    setEditing(classroom)
    setForm({ 
      gradeLevel: classroom.gradeLevel, 
      line: classroom.line, 
      schoolId: classroom.schoolId, 
      tutorId: classroom.tutorId || '' 
    })
    setShowForm(true)
  }

  const gradeLabel = (g: number) => {
    switch(g) {
      case 1: return "Infantil 3 años"
      case 2: return "Infantil 4 años"
      case 3: return "Infantil 5 años"
      case 4: return "1º Primaria"
      case 5: return "2º Primaria"
      case 6: return "3º Primaria"
      case 7: return "4º Primaria"
      case 8: return "5º Primaria"
      case 9: return "6º Primaria"
      default: return `${g}º Grado`
    }
  }

  const getLevelColor = (g: number) => {
    if (g <= 3) return 'bg-pink-50 text-pink-600 border-pink-100'
    return 'bg-blue-50 text-blue-600 border-blue-100'
  }

  const lineLabel = (l: string | number) => {
    if (l === 0 || l === '0' || l === 'A') return 'A'
    if (l === 1 || l === '1' || l === 'B') return 'B'
    if (l === 2 || l === '2' || l === 'C') return 'C'
    return l
  }

  const filteredClassrooms = classrooms.filter(c => 
    gradeLabel(c.gradeLevel).toLowerCase().includes(searchTerm.toLowerCase()) ||
    lineLabel(c.line).toLowerCase().includes(searchTerm.toLowerCase()) ||
    (`${c.tutor?.firstName} ${c.tutor?.lastName}`).toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="space-y-6"
    >
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="relative w-full sm:w-96">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-surface-400" size={18} />
          <input 
            type="text" 
            placeholder="Buscar aula, nivel o tutor..." 
            className="input-field w-full pl-10"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <button 
          onClick={() => { setShowForm(true); setEditing(null); }} 
          className="btn-primary flex items-center gap-2 w-full sm:w-auto justify-center"
        >
          <Plus size={20} />
          Nueva Aula
        </button>
      </div>

      <div className="glass-card overflow-hidden">
        {loading ? (
          <div className="p-12 text-center">
            <div className="animate-spin w-8 h-8 border-4 border-brand-600 border-t-transparent rounded-full mx-auto mb-4"></div>
            <p className="text-surface-500 font-medium">Cargando aulas...</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-surface-50 border-b border-surface-200">
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Nivel Educativo</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Línea</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Tutor</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Alumnos</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-surface-100">
                <AnimatePresence>
                  {filteredClassrooms.map((c) => (
                    <motion.tr 
                      key={c.id}
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      onDoubleClick={() => setSelectedClassroom(c)}
                      className="hover:bg-brand-50/20 transition-colors group cursor-pointer select-none"
                      title="Doble clic para ver alumnos"
                    >
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <div className={`w-10 h-10 rounded-xl border flex items-center justify-center font-bold ${getLevelColor(c.gradeLevel)}`}>
                            <DoorOpen size={20} />
                          </div>
                          <span className="font-semibold text-surface-900">{gradeLabel(c.gradeLevel)}</span>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <span className="px-3 py-1 rounded-lg bg-surface-100 text-surface-700 font-bold text-sm">
                          {lineLabel(c.line)}
                        </span>
                      </td>
                      <td className="px-6 py-4">
                        {c.tutor ? (
                          <div className="flex items-center gap-2">
                            <div className="w-7 h-7 rounded-full bg-brand-100 flex items-center justify-center text-brand-600">
                              <GraduationCap size={14} />
                            </div>
                            <span className="text-sm font-medium text-surface-700">{c.tutor.firstName} {c.tutor.lastName}</span>
                          </div>
                        ) : (
                          <span className="text-sm text-surface-400 italic">Sin tutor</span>
                        )}
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-2 text-sm text-surface-600">
                          <Users size={16} className="text-surface-400" />
                          <span className="font-semibold">{c.students?.length || 0}</span>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                          <button 
                            onClick={(e) => handleEdit(c, e)}
                            className="p-2 hover:bg-brand-50 text-brand-600 rounded-lg transition-colors"
                          >
                            <Edit2 size={16} />
                          </button>
                          <button 
                            onClick={(e) => handleDelete(c.id, e)}
                            className="p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </motion.tr>
                  ))}
                </AnimatePresence>
                {!loading && filteredClassrooms.length === 0 && (
                  <tr>
                    <td colSpan={5} className="px-6 py-12 text-center text-surface-400">
                      No se encontraron aulas.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Classroom Details Slide-over */}
      <AnimatePresence>
        {selectedClassroom && (
          <div className="fixed inset-0 z-[110] flex justify-end">
            <motion.div 
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={() => setSelectedClassroom(null)}
              className="absolute inset-0 bg-surface-900/40 backdrop-blur-sm"
            />
            <motion.div 
              initial={{ x: '100%' }}
              animate={{ x: 0 }}
              exit={{ x: '100%' }}
              transition={{ type: 'spring', damping: 25, stiffness: 200 }}
              className="bg-white w-full max-w-xl relative shrink-0 shadow-2xl flex flex-col"
            >
              <div className="p-6 border-b border-surface-100 flex items-center justify-between bg-surface-50">
                <div>
                  <h3 className="text-xl font-bold flex items-center gap-2">
                    <Layers className="text-brand-600" size={24} />
                    Aula {gradeLabel(selectedClassroom.gradeLevel)} - {lineLabel(selectedClassroom.line)}
                  </h3>
                  <p className="text-sm text-surface-500">Listado de alumnos matriculados</p>
                </div>
                <button onClick={() => setSelectedClassroom(null)} className="p-2 hover:bg-surface-200 rounded-full transition-colors">
                  <X size={20} />
                </button>
              </div>

              <div className="flex-1 overflow-y-auto p-6 space-y-6">
                {/* Tutor Card */}
                <div className="p-4 rounded-2xl bg-brand-50/50 border border-brand-100 flex items-center gap-4">
                  <div className="w-12 h-12 rounded-xl bg-brand-600 flex items-center justify-center text-white">
                    <GraduationCap size={24} />
                  </div>
                  <div>
                    <p className="text-xs font-bold text-brand-600 uppercase tracking-wider">Tutor Principal</p>
                    <p className="font-bold text-surface-900">
                      {selectedClassroom.tutor ? `${selectedClassroom.tutor.firstName} ${selectedClassroom.tutor.lastName}` : 'No asignado'}
                    </p>
                  </div>
                </div>

                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <h4 className="font-bold text-surface-800 flex items-center gap-2">
                      <Users size={18} className="text-surface-400" />
                      Alumnos ({selectedClassroom.students?.length || 0})
                    </h4>
                  </div>

                  <div className="grid gap-3">
                    {selectedClassroom.students?.map((student, idx) => (
                      <motion.div 
                        key={student.id}
                        initial={{ opacity: 0, x: 20 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ delay: idx * 0.03 }}
                        className="p-4 rounded-xl border border-surface-100 hover:border-brand-200 hover:bg-surface-50 transition-all flex items-center gap-4 group"
                      >
                        <div className="w-10 h-10 rounded-full bg-surface-200 flex items-center justify-center text-surface-600 font-bold overflow-hidden">
                          <img 
                            src={`https://api.dicebear.com/7.x/avataaars/svg?seed=${student.firstName}${student.lastName}`} 
                            alt="Avatar"
                            className="w-full h-full object-cover"
                          />
                        </div>
                        <div className="flex-1">
                          <p className="font-bold text-surface-900">{student.firstName} {student.lastName}</p>
                          <p className="text-xs text-surface-500 flex items-center gap-1">
                            <Calendar size={12} />
                            F. Nacimiento: {new Date(student.dateOfBirth).toLocaleDateString()}
                          </p>
                        </div>
                        <button className="p-2 opacity-0 group-hover:opacity-100 transition-opacity hover:bg-white rounded-lg border border-transparent hover:border-surface-200 text-surface-400 hover:text-brand-600">
                          <UserSquare2 size={18} />
                        </button>
                      </motion.div>
                    ))}
                    {(!selectedClassroom.students || selectedClassroom.students.length === 0) && (
                      <div className="p-12 text-center text-surface-400">
                        No hay alumnos registrados en esta aula.
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>

      <AnimatePresence>
        {showForm && (
          <div className="fixed inset-0 z-[120] flex items-center justify-center p-4">
            <motion.div 
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={resetForm}
              className="absolute inset-0 bg-surface-900/40 backdrop-blur-sm"
            />
            <motion.div 
              initial={{ scale: 0.95, opacity: 0, y: 20 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              exit={{ scale: 0.95, opacity: 0, y: 20 }}
              className="bg-white rounded-2xl shadow-2xl w-full max-w-lg relative z-10 overflow-hidden"
            >
              <div className="p-6 border-b border-surface-100 flex items-center justify-between bg-surface-50">
                <h3 className="text-xl font-bold">{editing ? 'Editar Aula' : 'Nueva Aula'}</h3>
                <button onClick={resetForm} className="p-2 hover:bg-surface-200 rounded-full transition-colors">
                  <X size={20} />
                </button>
              </div>
              <form onSubmit={handleSubmit} className="p-8 space-y-6">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-surface-700">Nivel Educativo</label>
                    <select 
                      value={form.gradeLevel} 
                      onChange={e => setForm({ ...form, gradeLevel: Number(e.target.value) })} 
                      className="input-field w-full"
                    >
                      {[1,2,3,4,5,6,7,8,9].map(g => <option key={g} value={g}>{gradeLabel(g)}</option>)}
                    </select>
                  </div>
                  <div className="space-y-1.5">
                    <label className="text-sm font-semibold text-surface-700">Línea</label>
                    <select 
                      value={form.line} 
                      onChange={e => setForm({ ...form, line: e.target.value })} 
                      className="input-field w-full"
                    >
                      <option value="A">Línea A</option>
                      <option value="B">Línea B</option>
                      <option value="C">Línea C</option>
                    </select>
                  </div>
                </div>
                <div className="space-y-1.5">
                  <label className="text-sm font-semibold text-surface-700">Centro Educativo (ID)</label>
                  <input 
                    value={form.schoolId} 
                    onChange={e => setForm({ ...form, schoolId: e.target.value })} 
                    placeholder="ID del Colegio" 
                    className="input-field w-full" 
                  />
                </div>
                <div className="space-y-1.5">
                  <label className="text-sm font-semibold text-surface-700">Tutor Principal (ID - Opcional)</label>
                  <input 
                    value={form.tutorId} 
                    onChange={e => setForm({ ...form, tutorId: e.target.value })} 
                    placeholder="ID del Tutor" 
                    className="input-field w-full" 
                  />
                </div>
                <div className="flex gap-3 pt-4">
                  <button type="button" onClick={resetForm} className="btn-secondary flex-1">Cancelar</button>
                  <button type="submit" className="btn-primary flex-1">
                    {editing ? 'Guardar Cambios' : 'Crear Aula'}
                  </button>
                </div>
              </form>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </motion.div>
  )
}