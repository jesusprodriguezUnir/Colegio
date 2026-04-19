/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          50: '#f5f7ff',
          100: '#ebf0ff',
          200: '#dae3ff',
          300: '#bbcbff',
          400: '#91a8ff',
          500: '#637eff',
          600: '#3b54f6',
          700: '#2537e4',
          800: '#232fba',
          900: '#222d93',
          950: '#141a57',
        },
        surface: {
          50: '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
        }
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui'],
        display: ['Outfit', 'Inter', 'ui-sans-serif'],
      },
      borderRadius: {
        'xl': '1rem',
        '2xl': '1.5rem',
      },
      boxShadow: {
        'premium': '0 10px 30px -10px rgba(0, 0, 0, 0.1), 0 1px 4px -1px rgba(0, 0, 0, 0.02)',
        'premium-hover': '0 20px 40px -15px rgba(0, 0, 0, 0.15), 0 1px 10px -1px rgba(0, 0, 0, 0.05)',
      }
    },
  },
  plugins: [],
}