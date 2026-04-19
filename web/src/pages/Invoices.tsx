import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { 
  Plus, 
  Search, 
  Edit2, 
  Trash2, 
  Receipt, 
  Calendar,
  X,
  CreditCard,
  CheckCircle2,
  AlertCircle,
  Filter,
  MoreVertical,
  Banknote
} from 'lucide-react'
import { invoicesApi } from '../services/api'
import type { Invoice } from '../types'

export default function Invoices() {
  const [invoices, setInvoices] = useState<Invoice[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [editing, setEditing] = useState<Invoice | null>(null)
  const [filter, setFilter] = useState<'all' | 'Pending' | 'Paid'>('all')
  const [form, setForm] = useState({ parentId: '', studentId: '', totalAmount: 350, concept: 'Monthly' as const })

  useEffect(() => {
    loadInvoices()
  }, [])

  const loadInvoices = async () => {
    try {
      const res = await invoicesApi.getAll()
      setInvoices(res.data)
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    const today = new Date()
    const data = {
      ...form,
      issueDate: today.toISOString(),
      dueDate: new Date(today.getTime() + 30 * 24 * 60 * 60 * 1000).toISOString(),
      status: 'Pending' as const,
    }
    try {
      if (editing) {
        await invoicesApi.update(editing.id, { ...form })
      } else {
        await invoicesApi.create(data)
      }
      resetForm()
      loadInvoices()
    } catch (e) {
      console.error(e)
    }
  }

  const resetForm = () => {
    setForm({ parentId: '', studentId: '', totalAmount: 350, concept: 'Monthly' })
    setShowForm(false)
    setEditing(null)
  }

  const handleDelete = async (id: string) => {
    if (confirm('¿Deseas anular esta factura?')) {
      try {
        await invoicesApi.delete(id)
        loadInvoices()
      } catch (e) {
        console.error(e)
      }
    }
  }

  const handleEdit = (invoice: Invoice) => {
    setEditing(invoice)
    setForm({ parentId: invoice.parentId, studentId: invoice.studentId, totalAmount: invoice.totalAmount, concept: invoice.concept })
    setShowForm(true)
  }

  const handleMarkPaid = async (id: string) => {
    try {
      await invoicesApi.update(id, { status: 'Paid' })
      loadInvoices()
    } catch (e) {
      console.error(e)
    }
  }

  const conceptLabel = (c: string) => {
    switch(c) {
      case 'Monthly': return 'Cuota Mensual'
      case 'Lunch': return 'Servicio Comedor'
      case 'Extracurricular': return 'Act. Extraescolar'
      default: return c
    }
  }

  const filtered = invoices.filter(i => filter === 'all' || i.status === filter)

  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="space-y-6"
    >
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="flex items-center gap-2 p-1 bg-surface-100 rounded-xl">
          {(['all', 'Pending', 'Paid'] as const).map((f) => (
            <button
              key={f}
              onClick={() => setFilter(f)}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                filter === f 
                  ? 'bg-white text-surface-900 shadow-sm' 
                  : 'text-surface-500 hover:text-surface-700'
              }`}
            >
              {f === 'all' ? 'Todas' : f === 'Pending' ? 'Pendientes' : 'Pagadas'}
            </button>
          ))}
        </div>
        <button 
          onClick={() => { setShowForm(true); setEditing(null); }} 
          className="btn-primary flex items-center gap-2 w-full sm:w-auto justify-center"
        >
          <Plus size={20} />
          Crear Factura
        </button>
      </div>

      <div className="glass-card overflow-hidden">
        {loading ? (
          <div className="p-12 text-center">
            <div className="animate-spin w-8 h-8 border-4 border-brand-600 border-t-transparent rounded-full mx-auto mb-4"></div>
            <p className="text-surface-500 font-medium">Cargando transacciones...</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-surface-50 border-b border-surface-200">
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Concepto / Fecha</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Cliente / Alumno</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Importe</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider">Estado</th>
                  <th className="px-6 py-4 text-xs font-bold text-surface-500 uppercase tracking-wider text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-surface-100">
                <AnimatePresence>
                  {filtered.map((inv) => (
                    <motion.tr 
                      key={inv.id}
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      className="hover:bg-surface-50/50 transition-colors group"
                    >
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <div className={`w-10 h-10 rounded-xl flex items-center justify-center shrink-0 ${
                            inv.status === 'Paid' ? 'bg-emerald-50 text-emerald-600' : 'bg-amber-50 text-amber-600'
                          }`}>
                            <Receipt size={20} />
                          </div>
                          <div className="flex flex-col">
                            <span className="font-semibold text-surface-900">{conceptLabel(inv.concept)}</span>
                            <span className="text-xs text-surface-400">Emisión: {new Date(inv.issueDate).toLocaleDateString()}</span>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex flex-col text-sm">
                          <span className="text-surface-700 font-medium">Tutor: {inv.parentId.split('-')[0]}...</span>
                          <span className="text-xs text-surface-500">Alumno: {inv.studentId.split('-')[0]}...</span>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <span className="font-bold text-surface-900">€{inv.totalAmount.toLocaleString()}</span>
                      </td>
                      <td className="px-6 py-4">
                        <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold ${
                          inv.status === 'Paid' 
                            ? 'bg-emerald-50 text-emerald-700' 
                            : 'bg-amber-50 text-amber-700'
                        }`}>
                          {inv.status === 'Paid' ? <CheckCircle2 size={12} /> : <AlertCircle size={12} />}
                          {inv.status === 'Paid' ? 'Pagada' : 'Pendiente'}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex items-center justify-end gap-2">
                          {inv.status === 'Pending' && (
                            <button 
                              onClick={() => handleMarkPaid(inv.id)}
                              className="text-xs font-bold text-emerald-600 hover:bg-emerald-50 px-3 py-1.5 rounded-lg transition-colors"
                            >
                              Pagar
                            </button>
                          )}
                          <div className="opacity-0 group-hover:opacity-100 transition-opacity flex gap-1">
                            <button onClick={() => handleEdit(inv)} className="p-2 hover:bg-surface-100 text-surface-500 rounded-lg">
                              <Edit2 size={16} />
                            </button>
                            <button onClick={() => handleDelete(inv.id)} className="p-2 hover:bg-red-50 text-red-600 rounded-lg">
                              <Trash2 size={16} />
                            </button>
                          </div>
                        </div>
                      </td>
                    </motion.tr>
                  ))}
                </AnimatePresence>
                {!loading && filtered.length === 0 && (
                  <tr>
                    <td colSpan={5} className="px-6 py-12 text-center text-surface-400">
                      No hay facturas que coincidan con el filtro.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <AnimatePresence>
        {showForm && (
          <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
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
                <h3 className="text-xl font-bold">Nueva Factura</h3>
                <button onClick={resetForm} className="p-2 hover:bg-surface-200 rounded-full transition-colors">
                  <X size={20} />
                </button>
              </div>
              <form onSubmit={handleSubmit} className="p-8 space-y-6">
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">ID Tutor / Pagador</label>
                      <input 
                        value={form.parentId} 
                        onChange={e => setForm({ ...form, parentId: e.target.value })} 
                        placeholder="ID Tutor" 
                        className="input-field w-full" 
                        required 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">ID Alumno</label>
                      <input 
                        value={form.studentId} 
                        onChange={e => setForm({ ...form, studentId: e.target.value })} 
                        placeholder="ID Alumno" 
                        className="input-field w-full" 
                        required 
                      />
                    </div>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">Importe (€)</label>
                      <div className="relative">
                        <Banknote className="absolute left-3 top-1/2 -translate-y-1/2 text-surface-400" size={16} />
                        <input 
                          type="number"
                          value={form.totalAmount} 
                          onChange={e => setForm({ ...form, totalAmount: Number(e.target.value) })} 
                          className="input-field w-full pl-10" 
                        />
                      </div>
                    </div>
                    <div className="space-y-1.5">
                      <label className="text-sm font-semibold text-surface-700">Concepto</label>
                      <select 
                        value={form.concept} 
                        onChange={e => setForm({ ...form, concept: e.target.value as any })} 
                        className="input-field w-full"
                      >
                        <option value="Monthly">Mensualidad</option>
                        <option value="Lunch">Comedor</option>
                        <option value="Extracurricular">Extraescolares</option>
                      </select>
                    </div>
                  </div>
                </div>
                <div className="flex gap-3 pt-4">
                  <button type="button" onClick={resetForm} className="btn-secondary flex-1">Cancelar</button>
                  <button type="submit" className="btn-primary flex-1">Emitir Factura</button>
                </div>
              </form>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </motion.div>
  )
}