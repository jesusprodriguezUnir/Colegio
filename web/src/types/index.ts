export interface School {
  id: string
  name: string
  address: string
  contactPhone: string
  contactEmail: string
}

export interface Teacher {
  id: string
  firstName: string
  lastName: string
  specialty: string
  hireDate: string
}

export interface Student {
  id: string
  firstName: string
  lastName: string
  dateOfBirth: string
  classroomId?: string
}

export interface Classroom {
  id: string
  gradeLevel: number
  line: string
  schoolId: string
  tutorId?: string
}

export interface Invoice {
  id: string
  parentId: string
  studentId: string
  issueDate: string
  dueDate: string
  totalAmount: number
  status: 'Pending' | 'Paid'
  concept: 'Monthly' | 'Lunch' | 'Extracurricular'
}