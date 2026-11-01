// @ts-nocheck
import React, { useState, useMemo, useEffect } from 'react';
import {
  ShieldCheck,
  AlertTriangle,
  QrCode,
  Plus,
  Search,
  MapPin,
  Phone,
  CheckCircle2,
  Clock,
  FileText,
  Syringe,
  Heart,
  User,
  LogOut,
  ChevronRight,
  Copy,
  Check,
  Sparkles,
  Smartphone,
  Monitor,
  Code2,
  Bell,
  Filter,
  Navigation,
  Edit3,
  Trash2,
  X,
  Eye,
  EyeOff,
  Lock,
  Mail,
  CreditCard,
  Award,
  Activity,
  Calendar,
  Building2,
  Stethoscope,
  PawPrint,
  Info,
  Share2,
  Radio,
  Layers,
  CheckSquare,
  RefreshCw,
  ArrowUpRight
} from 'lucide-react';

// ============================================================================
// 1. INITIAL MOCK DATA (Aligned with HappyPet.Domain & AIRS / Tbilisi AMA)
// ============================================================================

const INITIAL_PETS = [
  {
    id: 'pet-1',
    name: 'ბაქსი (Baksi)',
    petType: 'Dog', // Enum: Dog, Cat, Other
    petTypeLabel: 'ძაღლი',
    gender: 'Male', // Enum: Male, Female
    genderLabel: 'ხვადი',
    breed: 'ოქროსფერი რეტრივერი (Golden Retriever)',
    age: '2 წლის და 4 თვის',
    birthDate: '2024-06-15',
    weight: '31.5 კგ',
    microchipCode: '900182001948271', // 15-digit ISO standard
    collarNumber: 'AMA-2026-4819',
    visualCharacteristics: 'ოქროსფერი ბეწვი, თეთრი ლაქა მკერდზე, მარცხენა ყურთან მცირე ხალი. ატარებს მწვანე ამრეკლავ საყელოს.',
    isSterilized: true,
    sterilizationDate: '2025-02-10',
    sterilizationClinic: 'ვეტ-ჰაუსი თბილისი (Vet House)',
    airsVerified: true,
    airsSyncDate: '2026-10-24',
    rabiesStatus: 'Active', // Active, Expiring, Expired
    rabiesValidUntil: '2027-05-14',
    ownerName: 'გოგიტა ჯღარკავა',
    ownerPersonalId: '0102408****',
    ownerPhone: '+995 599 12 34 56',
    isLost: false,
    avatarUrl: 'https://images.unsplash.com/photo-1552053831-71594a27632d?auto=format&fit=crop&w=600&q=80',
    vaccinations: [
      {
        id: 'vac-1',
        name: 'ცოფის საწინააღმდეგო (Nobivac Rabies)',
        type: 'Rabies',
        batchNumber: 'NB-99412-A',
        dateAdministered: '2026-05-14',
        nextDueDate: '2027-05-14',
        clinic: 'აგრარული უნივერსიტეტის ვეტ-კლინიკა',
        vetName: 'დოქ. ლევან კვარაცხელია',
        status: 'Active'
      },
      {
        id: 'vac-2',
        name: 'კომპლექსური ვაქცინა (DHPPi + L)',
        type: 'Complex',
        batchNumber: 'anguard-8821',
        dateAdministered: '2026-04-02',
        nextDueDate: '2027-04-02',
        clinic: 'ვეტ-ჰაუსი თბილისი',
        vetName: 'დოქ. ნინო ბერიძე',
        status: 'Active'
      }
    ],
    medicalRecords: [
      {
        id: 'med-1',
        date: '2026-09-18',
        title: 'გეგმიური ექოსკოპია და სისხლის ანალიზი',
        category: 'Checkup',
        clinic: 'ვეტ-ჰაუსი თბილისი',
        vetName: 'დოქ. ნინო ბერიძე',
        notes: 'ყველა მაჩვენებელი ნორმაშია. რეკომენდებულია დეჰელმინთიზაცია 3 თვეში ერთხელ.'
      },
      {
        id: 'med-2',
        date: '2025-02-10',
        title: 'კასტრაცია და AIRS მიკროჩიპირება',
        category: 'Surgery',
        clinic: 'თბილისის მუნიციპალური ლაბორატორია / AMA',
        vetName: 'დოქ. გიორგი მელაძე',
        notes: 'ჩადგმულია 15-ნიშნა ISO სტანდარტის ჩიპი (900182001948271). ოპერაციამ ჩაიარა გართულების გარეშე.'
      }
    ]
  },
  {
    id: 'pet-2',
    name: 'ლუნა (Luna)',
    petType: 'Cat',
    petTypeLabel: 'კატა',
    gender: 'Female',
    genderLabel: 'ძუ',
    breed: 'ბრიტანული მოკლებეწვიანი (British Shorthair)',
    age: '1 წლის და 8 თვის',
    birthDate: '2025-02-20',
    weight: '4.2 კგ',
    microchipCode: '900182005531904',
    collarNumber: 'AMA-2026-9102',
    visualCharacteristics: 'ნაცრისფერი-მოლურჯო ბეწვი, ქარვისფერი თვალები, ვარდისფერი საყელო ზარით.',
    isSterilized: true,
    sterilizationDate: '2025-11-05',
    sterilizationClinic: 'ბალტო - ვეტერინარული კლინიკა',
    airsVerified: true,
    airsSyncDate: '2026-10-20',
    rabiesStatus: 'Active',
    rabiesValidUntil: '2027-03-10',
    ownerName: 'გოგიტა ჯღარკავა',
    ownerPersonalId: '0102408****',
    ownerPhone: '+995 599 12 34 56',
    isLost: false,
    avatarUrl: 'https://images.unsplash.com/photo-1573865526739-10659fec78a5?auto=format&fit=crop&w=600&q=80',
    vaccinations: [
      {
        id: 'vac-3',
        name: 'ცოფის და პანლეიკოპენიის (Purevax RCP + Rabies)',
        type: 'Rabies',
        batchNumber: 'PVX-4410',
        dateAdministered: '2026-03-10',
        nextDueDate: '2027-03-10',
        clinic: 'ბალტო - საბურთალოს ფილიალი',
        vetName: 'დოქ. თამარ მაისურაძე',
        status: 'Active'
      }
    ],
    medicalRecords: [
      {
        id: 'med-3',
        date: '2025-11-05',
        title: 'სტერილიზაცია (ლაპაროსკოპიული)',
        category: 'Surgery',
        clinic: 'ბალტო - საბურთალოს ფილიალი',
        vetName: 'დოქ. თამარ მაისურაძე',
        notes: 'პოსტოპერაციული რეაბილიტაცია დასრულებულია წარმატებით.'
      }
    ]
  }
];

const INITIAL_SOS_ALERTS = [
  {
    id: 'sos-1',
    type: 'LOST', // LOST or FOUND
    petName: 'ჩარლი (Charlie)',
    petType: 'Dog',
    breed: 'ჰასკი (Siberian Husky)',
    gender: 'Male',
    microchipCode: '900182008812309',
    collarNumber: 'AMA-2026-1108',
    neighborhood: 'ვაკის პარკი, თბილისი',
    distanceKm: '0.8 კმ',
    lastSeenTime: '25 წუთის წინ',
    description: 'ცისფერი თვალებით, წითელი ტყავის საყელოთი. დაფრთხა ფოიერვერკის ხმაზე ვაკის პარკის შადრევანთან.',
    contactPhone: '+995 577 90 11 22',
    ownerName: 'ირაკლი მაჭარაშვილი',
    reward: '300 ₾',
    coords: { x: 34, y: 58 }, // Simulated radar position %
    avatarUrl: 'https://images.unsplash.com/photo-1605568427561-40dd23c2acea?auto=format&fit=crop&w=600&q=80',
    sightingsCount: 3,
    airsVerified: true
  },
  {
    id: 'sos-2',
    type: 'LOST',
    petName: 'მია (Mia)',
    petType: 'Cat',
    breed: 'სიამის კატა (Siamese)',
    gender: 'Female',
    microchipCode: '900182003349182',
    collarNumber: 'AMA-2026-7741',
    neighborhood: 'საბურთალო, პეკინის გამზ. #24',
    distanceKm: '2.1 კმ',
    lastSeenTime: '2 საათის წინ',
    description: 'ძალიან მშიშარაა, ატარებს ლურჯ GPS საყელოს (ელემენტი დამჯდარია). რეაგირებს სახელზე „მია“.',
    contactPhone: '+995 591 44 55 66',
    ownerName: 'თამარ ჩინჩალაძე',
    reward: '150 ₾',
    coords: { x: 52, y: 36 },
    avatarUrl: 'https://images.unsplash.com/photo-1513360371669-4adf3dd7dff8?auto=format&fit=crop&w=600&q=80',
    sightingsCount: 1,
    airsVerified: true
  },
  {
    id: 'sos-3',
    type: 'FOUND',
    petName: 'ნაპოვნია კოკერ-სპანიელი',
    petType: 'Dog',
    breed: 'ინგლისური კოკერ-სპანიელი',
    gender: 'Male',
    microchipCode: '900182007719283',
    collarNumber: 'AMA-2026-3092',
    neighborhood: 'მთაწმინდა, რუსთაველის მეტროსთან',
    distanceKm: '3.4 კმ',
    lastSeenTime: '45 წუთის წინ',
    description: 'ნაპოვნია მოვლილი ხვადი სპანიელი. ჩიპი დასკანერდა ვეტ-კლინიკაში, ველოდებით პატრონთან დაკავშირებას.',
    contactPhone: '+995 598 00 11 99',
    ownerName: 'კლინიკა „აიბოლიტი“ / AMA',
    reward: null,
    coords: { x: 74, y: 64 },
    avatarUrl: 'https://images.unsplash.com/photo-1537151608828-ea2b11777ee8?auto=format&fit=crop&w=600&q=80',
    sightingsCount: 5,
    airsVerified: true
  }
];

const TBILISI_NEIGHBORHOODS = [
  'ვაკე - ვაკის პარკის მიმდებარედ',
  'საბურთალო - დელისის მეტრო / პეკინი',
  'მთაწმინდა - რუსთაველის გამზირი',
  'დიღმის მასივი / დიდი დიღომი',
  'გლდანი - მე-3 მიკრორაიონი',
  'ისანი - ავლაბრის მოედანი',
  'ლისის ტბის მიმდებარე ტერიტორია'
];

// ============================================================================
// 2. MAIN APPLICATION COMPONENT
// ============================================================================

export default function App() {
  // --- Global UI & Handoff Controls ---
  const [viewportMode, setViewportMode] = useState('mobile'); // 'mobile' | 'full' | 'specs'
  const [activeTab, setActiveTab] = useState('home'); // 'home' | 'passport' | 'radar' | 'profile' | 'auth'
  const [userRole, setUserRole] = useState('Owner'); // 'Owner' | 'Vet' | 'Shelter'

  // --- Auth State (Simulating AuthContext with JWT) ---
  const [authUser, setAuthUser] = useState({
    id: 'usr-901',
    fullName: 'გოგიტა ჯღარკავა',
    email: 'gogita.jgarkava@happypet.ge',
    personalNumber: '01024085912',
    phone: '+995 599 12 34 56',
    role: 'Owner',
    jwtExpiry: '7 Days (HS256)',
    isVerifiedAirs: true
  });

  // --- Domain Data State (Simulating TanStack Query Cache) ---
  const [pets, setPets] = useState(INITIAL_PETS);
  const [selectedPetId, setSelectedPetId] = useState(INITIAL_PETS[0].id);
  const [sosAlerts, setSosAlerts] = useState(INITIAL_SOS_ALERTS);
  const [radarFilter, setRadarFilter] = useState('ALL'); // 'ALL' | 'LOST' | 'FOUND'
  const [selectedPinAlert, setSelectedPinAlert] = useState(INITIAL_SOS_ALERTS[0]);

  // --- Modals State ---
  const [isSosModalOpen, setIsSosModalOpen] = useState(false);
  const [isPetCrudModalOpen, setIsPetCrudModalOpen] = useState(false);
  const [editingPet, setEditingPet] = useState(null);
  const [petToDelete, setPetToDelete] = useState(null);
  const [isQrModalOpen, setIsQrModalOpen] = useState(false);
  const [isChipLookupModalOpen, setIsChipLookupModalOpen] = useState(false);
  const [isVetRecordModalOpen, setIsVetRecordModalOpen] = useState(false);
  const [activeSightingAlert, setActiveSightingAlert] = useState(null);

  // --- Toast Notifications (Simulating 'sonner') ---
  const [toasts, setToasts] = useState([]);

  const addToast = (title, description, type = 'success') => {
    const id = Date.now() + Math.random();
    setToasts((prev) => [...prev, { id, title, description, type }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 4200);
  };

  // --- Currently Selected Pet for Passport ---
  const selectedPet = useMemo(
    () => pets.find((p) => p.id === selectedPetId) || pets[0],
    [pets, selectedPetId]
  );

  // --- Copy helper ---
  const [copiedText, setCopiedText] = useState('');
  const handleCopy = (text, label) => {
    navigator.clipboard?.writeText(text);
    setCopiedText(text);
    addToast('დაკოპირდა ბუფერში', `${label}: ${text}`, 'info');
    setTimeout(() => setCopiedText(''), 2000);
  };

  // --- Role Switcher Handler ---
  const handleRoleChange = (newRole) => {
    setUserRole(newRole);
    setAuthUser((prev) => ({ ...prev, role: newRole }));
    const roleNames = {
      Owner: 'მესაკუთრე (Pet Owner)',
      Vet: 'ვეტ-კლინიკა (Authorized Veterinarian)',
      Shelter: 'თავშესაფარი / AMA ინსპექტორი'
    };
    addToast(
      `როლი შეიცვალა: ${roleNames[newRole]}`,
      'RBAC უფლებები და UI კომპონენტები განახლდა JWT Claim-ის შესაბამისად.',
      'info'
    );
  };

  // ============================================================================
  // 3. SUB-COMPONENTS & MODALS
  // ============================================================================

  // --- A. Report Lost Pet / SOS Modal (React Hook Form + Zod validation rules) ---
  const SosModal = () => {
    const [selectedRegisteredPetId, setSelectedRegisteredPetId] = useState(selectedPet?.id || '');
    const [alertType, setAlertType] = useState('LOST');
    const [petName, setPetName] = useState(selectedPet?.name || '');
    const [breed, setBreed] = useState(selectedPet?.breed || '');
    const [microchipCode, setMicrochipCode] = useState(selectedPet?.microchipCode || '');
    const [collarNumber, setCollarNumber] = useState(selectedPet?.collarNumber || '');
    const [neighborhood, setNeighborhood] = useState(TBILISI_NEIGHBORHOODS[0]);
    const [description, setDescription] = useState('');
    const [contactPhone, setContactPhone] = useState(authUser.phone);
    const [reward, setReward] = useState('200 ₾');
    const [errors, setErrors] = useState({});

    // Auto-fill when user picks one of their registered pets
    const handleSelectRegisteredPet = (id) => {
      setSelectedRegisteredPetId(id);
      if (id === 'manual') {
        setPetName('');
        setBreed('');
        setMicrochipCode('');
        setCollarNumber('');
        return;
      }
      const found = pets.find((p) => p.id === id);
      if (found) {
        setPetName(found.name);
        setBreed(found.breed);
        setMicrochipCode(found.microchipCode);
        setCollarNumber(found.collarNumber);
        setDescription(found.visualCharacteristics);
      }
    };

    const validateAndSubmit = (e) => {
      e.preventDefault();
      const newErrors = {};

      if (!petName || petName.trim().length < 2) {
        newErrors.petName = 'ცხოველის სახელი სავალდებულოა (მინ. 2 სიმბოლო)';
      }
      if (!/^\d{15}$/.test(microchipCode)) {
        newErrors.microchipCode = 'AIRS სტანდარტი: მიკროჩიპი უნდა შედგებოდეს ზუსტად 15 ციფრისგან';
      }
      if (!description || description.trim().length < 10) {
        newErrors.description = 'გთხოვთ დეტალურად აღწეროთ გარემოება და ნიშნები (მინ. 10 სიმბოლო)';
      }
      if (!contactPhone || contactPhone.trim().length < 9) {
        newErrors.contactPhone = 'მიუთითეთ აქტიური საკონტაქტო ნომერი';
      }

      if (Object.keys(newErrors).length > 0) {
        setErrors(newErrors);
        addToast('ვალიდაციის შეცდომა (Zod Schema)', 'გთხოვთ შეავსოთ სავალდებულო ველები სწორად.', 'error');
        return;
      }

      const newAlert = {
        id: `sos-${Date.now()}`,
        type: alertType,
        petName,
        petType: 'Dog',
        breed: breed || 'მითითებული არ არის',
        gender: 'Male',
        microchipCode,
        collarNumber: collarNumber || 'AMA-უცნობი',
        neighborhood,
        distanceKm: '0.1 კმ (თქვენთან)',
        lastSeenTime: 'ახლახანს',
        description,
        contactPhone,
        ownerName: authUser.fullName,
        reward: reward.trim() ? reward : null,
        coords: {
          x: Math.floor(Math.random() * 55) + 20,
          y: Math.floor(Math.random() * 50) + 25
        },
        avatarUrl:
          pets.find((p) => p.id === selectedRegisteredPetId)?.avatarUrl ||
          'https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=600&q=80',
        sightingsCount: 1,
        airsVerified: true
      };

      setSosAlerts((prev) => [newAlert, ...prev]);
      setSelectedPinAlert(newAlert);

      if (selectedRegisteredPetId && selectedRegisteredPetId !== 'manual') {
        setPets((prev) =>
          prev.map((p) => (p.id === selectedRegisteredPetId ? { ...p, isLost: alertType === 'LOST' } : p))
        );
      }

      setIsSosModalOpen(false);
      setActiveTab('radar');
      addToast(
        '🚨 SOS განგაში გაიგზავნა! (Push Broadcast)',
        `${neighborhood}-ის 5 კმ რადიუსში მყოფ მომხმარებლებსა და AMA-ს ეკიპაჟებს გაეგზავნათ შეტყობინება.`,
        'sos'
      );
    };

    return (
      <div className="fixed inset-0 z-50 flex items-end sm:items-center justify-center bg-slate-900/70 backdrop-blur-sm p-0 sm:p-4 animate-fadeIn">
        <div className="bg-white w-full max-w-lg rounded-t-3xl sm:rounded-3xl shadow-2xl border border-slate-200 max-h-[92vh] flex flex-col overflow-hidden">
          {/* Header */}
          <div className="bg-gradient-to-r from-rose-600 to-rose-500 px-5 py-4 text-white flex items-center justify-between">
            <div className="flex items-center gap-2.5">
              <div className="p-2 bg-white/20 rounded-xl backdrop-blur-md">
                <Radio className="w-5 h-5 text-white animate-pulse" />
              </div>
              <div>
                <h3 className="font-bold text-base leading-tight">SOS განგაშის გამოცხადება</h3>
                <p className="text-xs text-rose-100">Push-ბროდქასთი თბილისის რადარსა და AIRS ქსელში</p>
              </div>
            </div>
            <button
              onClick={() => setIsSosModalOpen(false)}
              className="p-2 rounded-full bg-white/10 hover:bg-white/20 transition"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          {/* Form Body */}
          <form onSubmit={validateAndSubmit} className="p-5 space-y-4 overflow-y-auto text-slate-800">
            {/* Alert Type Toggle */}
            <div className="grid grid-cols-2 gap-2 p-1 bg-slate-100 rounded-xl">
              <button
                type="button"
                onClick={() => setAlertType('LOST')}
                className={`py-2.5 px-3 rounded-lg text-xs font-bold flex items-center justify-center gap-1.5 transition ${
                  alertType === 'LOST'
                    ? 'bg-rose-600 text-white shadow-sm'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                <AlertTriangle className="w-4 h-4" />
                დამეკარგა ცხოველი (LOST)
              </button>
              <button
                type="button"
                onClick={() => setAlertType('FOUND')}
                className={`py-2.5 px-3 rounded-lg text-xs font-bold flex items-center justify-center gap-1.5 transition ${
                  alertType === 'FOUND'
                    ? 'bg-teal-600 text-white shadow-sm'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                <Sparkles className="w-4 h-4" />
                ვიპოვე ცხოველი (FOUND)
              </button>
            </div>

            {/* Auto-fill from My Pets */}
            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1.5">
                აირჩიეთ რეგისტრირებული ცხოველი (Auto-fill AIRS ჩიპი)
              </label>
              <select
                value={selectedRegisteredPetId}
                onChange={(e) => handleSelectRegisteredPet(e.target.value)}
                className="w-full px-3.5 py-2.5 bg-slate-50 border border-slate-300 rounded-xl text-sm font-medium focus:ring-2 focus:ring-rose-500 focus:outline-none"
              >
                {pets.map((p) => (
                  <option key={p.id} value={p.id}>
                    🐾 {p.name} — ჩიპი: {p.microchipCode} ({p.collarNumber})
                  </option>
                ))}
                <option value="manual">➕ სხვა ცხოველის მონაცემების ხელით შეყვანა</option>
              </select>
            </div>

            {/* Name & Collar */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">
                  სახელი / იდენტიფიკატორი *
                </label>
                <input
                  type="text"
                  value={petName}
                  onChange={(e) => setPetName(e.target.value)}
                  placeholder="მაგ: ბაქსი"
                  className={`w-full px-3.5 py-2.5 rounded-xl border text-sm ${
                    errors.petName ? 'border-rose-500 bg-rose-50' : 'border-slate-300 bg-white'
                  }`}
                />
                {errors.petName && <p className="text-[11px] text-rose-600 mt-1">{errors.petName}</p>}
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">
                  საყელოს ნომერი (Tbilisi AMA)
                </label>
                <input
                  type="text"
                  value={collarNumber}
                  onChange={(e) => setCollarNumber(e.target.value)}
                  placeholder="AMA-2026-XXXX"
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-mono"
                />
              </div>
            </div>

            {/* 15-Digit Microchip Validation Field */}
            <div>
              <div className="flex items-center justify-between mb-1">
                <label className="text-xs font-bold text-slate-700">
                  15-ნიშნა მიკროჩიპის კოდი (ISO / AIRS) *
                </label>
                <span
                  className={`text-[11px] font-mono font-bold px-2 py-0.5 rounded-full ${
                    microchipCode.length === 15
                      ? 'bg-emerald-100 text-emerald-700'
                      : 'bg-amber-100 text-amber-700'
                  }`}
                >
                  {microchipCode.length} / 15 ციფრი
                </span>
              </div>
              <input
                type="text"
                maxLength={15}
                value={microchipCode}
                onChange={(e) => setMicrochipCode(e.target.value.replace(/\D/g, ''))}
                placeholder="90018200XXXXXXX"
                className={`w-full px-3.5 py-2.5 rounded-xl border font-mono text-sm tracking-wider ${
                  errors.microchipCode ? 'border-rose-500 bg-rose-50' : 'border-slate-300'
                }`}
              />
              {errors.microchipCode && (
                <p className="text-[11px] text-rose-600 mt-1 font-medium">{errors.microchipCode}</p>
              )}
            </div>

            {/* Location in Tbilisi */}
            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1">
                ბოლოს ნანახი მდებარეობა (თბილისის უბანი) *
              </label>
              <div className="relative">
                <MapPin className="w-4 h-4 text-rose-600 absolute left-3.5 top-3" />
                <select
                  value={neighborhood}
                  onChange={(e) => setNeighborhood(e.target.value)}
                  className="w-full pl-9 pr-3.5 py-2.5 bg-white border border-slate-300 rounded-xl text-sm"
                >
                  {TBILISI_NEIGHBORHOODS.map((loc) => (
                    <option key={loc} value={loc}>
                      {loc}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            {/* Visual Markings & Circumstances */}
            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1">
                ვიზუალური მახასიათებლები და დაკარგვის გარემოება *
              </label>
              <textarea
                rows={3}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="აღწერეთ საყელოს ფერი, განსაკუთრებული ნიშნები, ქცევა..."
                className={`w-full px-3.5 py-2 rounded-xl border text-sm ${
                  errors.description ? 'border-rose-500 bg-rose-50' : 'border-slate-300'
                }`}
              />
              {errors.description && (
                <p className="text-[11px] text-rose-600 mt-1">{errors.description}</p>
              )}
            </div>

            {/* Contact Phone & Reward */}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">
                  საგანგებო ტელეფონი *
                </label>
                <input
                  type="text"
                  value={contactPhone}
                  onChange={(e) => setContactPhone(e.target.value)}
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm"
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">
                  საჩუქარი / ჯილდო (არასავალდებულო)
                </label>
                <input
                  type="text"
                  value={reward}
                  onChange={(e) => setReward(e.target.value)}
                  placeholder="მაგ: 200 ₾"
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm"
                />
              </div>
            </div>

            {/* Submit Actions */}
            <div className="pt-2 flex gap-2.5">
              <button
                type="button"
                onClick={() => setIsSosModalOpen(false)}
                className="w-1/3 py-3 rounded-xl border border-slate-300 text-xs font-bold text-slate-700 hover:bg-slate-100"
              >
                გაუქმება
              </button>
              <button
                type="submit"
                className="w-2/3 py-3 rounded-xl bg-rose-600 hover:bg-rose-700 text-white text-xs font-bold shadow-lg shadow-rose-600/25 flex items-center justify-center gap-2 transition"
              >
                <Bell className="w-4 h-4 animate-bounce" />
                SOS განგაშის გაშვება (Push Broadcast)
              </button>
            </div>
          </form>
        </div>
      </div>
    );
  };

  // --- B. Pet CRUD Modal (Create & Update matching HappyPet.Domain Pet entity) ---
  const PetCrudModal = () => {
    const isEdit = Boolean(editingPet);
    const [name, setName] = useState(editingPet?.name || '');
    const [petType, setPetType] = useState(editingPet?.petType || 'Dog');
    const [gender, setGender] = useState(editingPet?.gender || 'Male');
    const [breed, setBreed] = useState(editingPet?.breed || '');
    const [age, setAge] = useState(editingPet?.age || '1 წლის');
    const [weight, setWeight] = useState(editingPet?.weight || '12 კგ');
    const [microchipCode, setMicrochipCode] = useState(editingPet?.microchipCode || '90018200');
    const [collarNumber, setCollarNumber] = useState(editingPet?.collarNumber || 'AMA-2026-');
    const [visualCharacteristics, setVisualCharacteristics] = useState(
      editingPet?.visualCharacteristics || ''
    );
    const [isSterilized, setIsSterilized] = useState(editingPet?.isSterilized ?? true);
    const [rabiesStatus, setRabiesStatus] = useState(editingPet?.rabiesStatus || 'Active');
    const [errors, setErrors] = useState({});

    const handleSavePet = (e) => {
      e.preventDefault();
      const errs = {};
      if (!name.trim()) errs.name = 'ცხოველის სახელი სავალდებულოა';
      if (!breed.trim()) errs.breed = 'ჯიშის მითითება სავალდებულოა';
      if (!/^\d{15}$/.test(microchipCode)) {
        errs.microchipCode = 'მიკროჩიპის კოდი უნდა შედგებოდეს ზუსტად 15 ციფრისგან (FluentValidation წესი)';
      }
      // Check uniqueness on create
      if (
        !isEdit &&
        pets.some((p) => p.microchipCode === microchipCode)
      ) {
        errs.microchipCode = 'ეს 15-ნიშნა მიკროჩიპი უკვე რეგისტრირებულია PostgreSQL ბაზაში (Unique Index)';
      }

      if (Object.keys(errs).length > 0) {
        setErrors(errs);
        addToast('შეცდომა ვალიდაციისას', 'შეამოწმეთ 15-ნიშნა ჩიპის კოდი და სავალდებულო ველები.', 'error');
        return;
      }

      const petPayload = {
        id: editingPet ? editingPet.id : `pet-${Date.now()}`,
        name,
        petType,
        petTypeLabel: petType === 'Dog' ? 'ძაღლი' : petType === 'Cat' ? 'კატა' : 'სხვა',
        gender,
        genderLabel: gender === 'Male' ? 'ხვადი' : 'ძუ',
        breed,
        age,
        birthDate: editingPet?.birthDate || '2025-05-01',
        weight,
        microchipCode,
        collarNumber: collarNumber || 'AMA-2026-0000',
        visualCharacteristics:
          visualCharacteristics || 'სტანდარტული შეფერილობა, იდენტიფიცირებულია AIRS ჩიპით.',
        isSterilized,
        sterilizationDate: isSterilized ? '2026-01-15' : null,
        sterilizationClinic: 'ვეტ-ჰაუსი თბილისი',
        airsVerified: true,
        airsSyncDate: '2026-11-01',
        rabiesStatus,
        rabiesValidUntil: '2027-08-01',
        ownerName: authUser.fullName,
        ownerPersonalId: '0102408****',
        ownerPhone: authUser.phone,
        isLost: editingPet?.isLost || false,
        avatarUrl:
          editingPet?.avatarUrl ||
          (petType === 'Cat'
            ? 'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?auto=format&fit=crop&w=600&q=80'
            : 'https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?auto=format&fit=crop&w=600&q=80'),
        vaccinations: editingPet?.vaccinations || [
          {
            id: `vac-${Date.now()}`,
            name: 'ცოფის საწინააღმდეგო (Nobivac Rabies)',
            type: 'Rabies',
            batchNumber: 'NB-2026-GE',
            dateAdministered: '2026-08-01',
            nextDueDate: '2027-08-01',
            clinic: 'თბილისის ვეტ-ცენტრი',
            vetName: 'დოქ. ლევან კვარაცხელია',
            status: rabiesStatus
          }
        ],
        medicalRecords: editingPet?.medicalRecords || [
          {
            id: `med-${Date.now()}`,
            date: '2026-11-01',
            title: 'პირველადი რეგისტრაცია და AIRS სინქრონიზაცია',
            category: 'Checkup',
            clinic: 'HappyPet Digital Registry',
            vetName: 'სისტემური რეგისტრატორი',
            notes: '15-ნიშნა მიკროჩიპი ვალიდირებულია სურსათის ეროვნული სააგენტოს (NFA) სტანდარტით.'
          }
        ]
      };

      if (isEdit) {
        setPets((prev) => prev.map((p) => (p.id === editingPet.id ? petPayload : p)));
        addToast('პროფილი განახლდა (PUT /api/pets)', `${name}-ის მონაცემები წარმატებით შეინახა.`, 'success');
      } else {
        setPets((prev) => [petPayload, ...prev]);
        setSelectedPetId(petPayload.id);
        addToast(
          'ცხოველი დარეგისტრირდა (POST /api/pets)',
          `${name} დაემატა და სინქრონიზდა AIRS ბაზასთან.`,
          'success'
        );
      }

      setIsPetCrudModalOpen(false);
      setEditingPet(null);
    };

    return (
      <div className="fixed inset-0 z-50 flex items-end sm:items-center justify-center bg-slate-900/70 backdrop-blur-sm p-0 sm:p-4 animate-fadeIn">
        <div className="bg-white w-full max-w-lg rounded-t-3xl sm:rounded-3xl shadow-2xl border border-slate-200 max-h-[92vh] flex flex-col overflow-hidden">
          <div className="bg-teal-600 px-5 py-4 text-white flex items-center justify-between">
            <div className="flex items-center gap-2.5">
              <div className="p-2 bg-white/20 rounded-xl">
                <PawPrint className="w-5 h-5 text-white" />
              </div>
              <div>
                <h3 className="font-bold text-base">
                  {isEdit ? 'ცხოველის მონაცემების რედაქტირება' : 'ახალი ცხოველის რეგისტრაცია (CRUD)'}
                </h3>
                <p className="text-xs text-teal-100">HappyPet.Domain • AIRS & AMA სტანდარტი</p>
              </div>
            </div>
            <button
              onClick={() => {
                setIsPetCrudModalOpen(false);
                setEditingPet(null);
              }}
              className="p-2 rounded-full bg-white/10 hover:bg-white/20"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          <form onSubmit={handleSavePet} className="p-5 space-y-4 overflow-y-auto text-slate-800">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">ცხოველის სახელი *</label>
                <input
                  type="text"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="მაგ: რეკსი (Rex)"
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm"
                />
                {errors.name && <p className="text-[11px] text-rose-600 mt-1">{errors.name}</p>}
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">სახეობა (PetType Enum)</label>
                <select
                  value={petType}
                  onChange={(e) => setPetType(e.target.value)}
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm bg-white"
                >
                  <option value="Dog">🐕 ძაღლი (Dog)</option>
                  <option value="Cat">🐈 კატა (Cat)</option>
                  <option value="Other">🐾 სხვა (Other)</option>
                </select>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">სქესი (Gender Enum)</label>
                <select
                  value={gender}
                  onChange={(e) => setGender(e.target.value)}
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm bg-white"
                >
                  <option value="Male">♂ ხვადი (Male)</option>
                  <option value="Female">♀ ძუ (Female)</option>
                </select>
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">ასაკი და წონა</label>
                <div className="grid grid-cols-2 gap-1.5">
                  <input
                    type="text"
                    value={age}
                    onChange={(e) => setAge(e.target.value)}
                    placeholder="2 წლის"
                    className="w-full px-2.5 py-2.5 rounded-xl border border-slate-300 text-xs"
                  />
                  <input
                    type="text"
                    value={weight}
                    onChange={(e) => setWeight(e.target.value)}
                    placeholder="15 კგ"
                    className="w-full px-2.5 py-2.5 rounded-xl border border-slate-300 text-xs"
                  />
                </div>
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1">ჯიში (Breed) *</label>
              <input
                type="text"
                value={breed}
                onChange={(e) => setBreed(e.target.value)}
                placeholder="მაგ: ლაბრადორ რეტრივერი / მეტისი"
                className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm"
              />
              {errors.breed && <p className="text-[11px] text-rose-600 mt-1">{errors.breed}</p>}
            </div>

            {/* 15-Digit Microchip Field with progress bar */}
            <div className="p-3.5 bg-slate-50 rounded-2xl border border-slate-200">
              <div className="flex items-center justify-between mb-1">
                <label className="text-xs font-bold text-slate-800 flex items-center gap-1.5">
                  <ShieldCheck className="w-4 h-4 text-teal-600" />
                  15-ნიშნა მიკროჩიპის კოდი (Unique Index) *
                </label>
                <span
                  className={`text-[11px] font-mono font-bold px-2 py-0.5 rounded-full ${
                    microchipCode.length === 15
                      ? 'bg-emerald-100 text-emerald-700'
                      : 'bg-amber-100 text-amber-700'
                  }`}
                >
                  {microchipCode.length}/15
                </span>
              </div>
              <div className="flex gap-2">
                <input
                  type="text"
                  maxLength={15}
                  value={microchipCode}
                  onChange={(e) => setMicrochipCode(e.target.value.replace(/\D/g, ''))}
                  className="w-full px-3.5 py-2 rounded-xl border border-slate-300 font-mono text-sm tracking-widest bg-white"
                />
                <button
                  type="button"
                  onClick={() => {
                    const random7 = Math.floor(1000000 + Math.random() * 9000000);
                    setMicrochipCode(`90018200${random7}`);
                  }}
                  className="px-3 py-2 bg-teal-50 text-teal-700 border border-teal-200 rounded-xl text-xs font-bold whitespace-nowrap hover:bg-teal-100"
                >
                  ISO გენერაცია
                </button>
              </div>
              {/* Progress Bar */}
              <div className="w-full h-1.5 bg-slate-200 rounded-full overflow-hidden mt-2">
                <div
                  className={`h-full transition-all duration-300 ${
                    microchipCode.length === 15 ? 'bg-emerald-500' : 'bg-teal-500'
                  }`}
                  style={{ width: `${(microchipCode.length / 15) * 100}%` }}
                />
              </div>
              {errors.microchipCode && (
                <p className="text-[11px] text-rose-600 mt-1.5 font-medium">{errors.microchipCode}</p>
              )}
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">
                  საყელოს № (Tbilisi AMA)
                </label>
                <input
                  type="text"
                  value={collarNumber}
                  onChange={(e) => setCollarNumber(e.target.value)}
                  placeholder="AMA-2026-4819"
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-mono"
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">
                  ცოფის ვაქცინაციის სტატუსი
                </label>
                <select
                  value={rabiesStatus}
                  onChange={(e) => setRabiesStatus(e.target.value)}
                  className="w-full px-3 py-2.5 rounded-xl border border-slate-300 text-sm bg-white"
                >
                  <option value="Active">✅ აქტიური (Active)</option>
                  <option value="Expiring">⚠️ ვადა ეწურება</option>
                  <option value="Expired">❌ ვადაგასული</option>
                </select>
              </div>
            </div>

            {/* Sterilization Checkbox */}
            <label className="flex items-center justify-between p-3 bg-emerald-50/70 border border-emerald-200 rounded-xl cursor-pointer">
              <div>
                <span className="text-xs font-bold text-slate-800 block">
                  სტერილიზებული / კასტრირებული (AMA სტანდარტი)
                </span>
                <span className="text-[11px] text-slate-500">
                  დადასტურებულია ვეტერინარული კლინიკის მიერ
                </span>
              </div>
              <input
                type="checkbox"
                checked={isSterilized}
                onChange={(e) => setIsSterilized(e.target.checked)}
                className="w-5 h-5 accent-teal-600 rounded"
              />
            </label>

            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1">
                ვიზუალური მახასიათებლები (ფერი, ლაქები, საყელო)
              </label>
              <textarea
                rows={2}
                value={visualCharacteristics}
                onChange={(e) => setVisualCharacteristics(e.target.value)}
                placeholder="მაგ: თეთრი ლაქა მკერდზე, მწვანე საყელო..."
                className="w-full px-3.5 py-2 rounded-xl border border-slate-300 text-sm"
              />
            </div>

            <div className="pt-2 flex gap-2.5">
              <button
                type="button"
                onClick={() => {
                  setIsPetCrudModalOpen(false);
                  setEditingPet(null);
                }}
                className="w-1/3 py-3 rounded-xl border border-slate-300 text-xs font-bold text-slate-700"
              >
                გაუქმება
              </button>
              <button
                type="submit"
                className="w-2/3 py-3 rounded-xl bg-teal-600 hover:bg-teal-700 text-white text-xs font-bold shadow-lg shadow-teal-600/25 transition"
              >
                {isEdit ? 'ცვლილებების შენახვა' : 'AIRS რეესტრში დამატება'}
              </button>
            </div>
          </form>
        </div>
      </div>
    );
  };

  // --- C. AIRS 15-Digit Microchip Scanner / Lookup Modal ---
  const ChipLookupModal = () => {
    const [queryChip, setQueryChip] = useState('900182001948271');
    const [lookupResult, setLookupResult] = useState(null);

    const runSearch = (codeToSearch) => {
      const clean = codeToSearch.replace(/\D/g, '');
      setQueryChip(clean);
      const foundPet = pets.find((p) => p.microchipCode === clean);
      const foundSos = sosAlerts.find((s) => s.microchipCode === clean);

      if (foundPet) {
        setLookupResult({ source: 'AIRS & HappyPet Registry', data: foundPet, isSos: foundPet.isLost });
      } else if (foundSos) {
        setLookupResult({
          source: 'AIRS Emergency SOS Database',
          data: {
            name: foundSos.petName,
            breed: foundSos.breed,
            microchipCode: foundSos.microchipCode,
            collarNumber: foundSos.collarNumber,
            ownerName: foundSos.ownerName,
            ownerPhone: foundSos.contactPhone,
            airsVerified: true
          },
          isSos: true
        });
      } else {
        setLookupResult({ source: 'NOT_FOUND' });
      }
    };

    useEffect(() => {
      runSearch('900182001948271');
    }, []);

    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/70 backdrop-blur-sm p-4 animate-fadeIn">
        <div className="bg-white w-full max-w-md rounded-3xl shadow-2xl border border-slate-200 overflow-hidden">
          <div className="bg-indigo-600 px-5 py-4 text-white flex items-center justify-between">
            <div className="flex items-center gap-2.5">
              <QrCode className="w-5 h-5" />
              <div>
                <h3 className="font-bold text-sm">AIRS / NFA 15-ნიშნა ჩიპის შემოწმება</h3>
                <p className="text-[11px] text-indigo-100">ერთიანი ეროვნული რეესტრის სიმულატორი</p>
              </div>
            </div>
            <button
              onClick={() => setIsChipLookupModalOpen(false)}
              className="p-1.5 rounded-full bg-white/10 hover:bg-white/20"
            >
              <X className="w-4 h-4" />
            </button>
          </div>

          <div className="p-5 space-y-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1">
                შეიყვანეთ 15-ნიშნა ISO მიკროჩიპის კოდი
              </label>
              <div className="flex gap-2">
                <input
                  type="text"
                  maxLength={15}
                  value={queryChip}
                  onChange={(e) => setQueryChip(e.target.value.replace(/\D/g, ''))}
                  className="w-full px-3.5 py-2.5 border border-slate-300 rounded-xl font-mono text-sm tracking-wider"
                />
                <button
                  onClick={() => runSearch(queryChip)}
                  className="px-4 py-2.5 bg-indigo-600 text-white rounded-xl text-xs font-bold hover:bg-indigo-700"
                >
                  ძებნა
                </button>
              </div>
              {/* Quick Test Chips */}
              <div className="flex flex-wrap gap-1.5 mt-2">
                <span className="text-[10px] text-slate-400 self-center">სატესტო კოდები:</span>
                <button
                  type="button"
                  onClick={() => runSearch('900182001948271')}
                  className="text-[10px] font-mono bg-slate-100 hover:bg-indigo-50 text-slate-700 px-2 py-0.5 rounded-md border border-slate-200"
                >
                  900182001948271 (ბაქსი)
                </button>
                <button
                  type="button"
                  onClick={() => runSearch('900182008812309')}
                  className="text-[10px] font-mono bg-rose-50 hover:bg-rose-100 text-rose-700 px-2 py-0.5 rounded-md border border-rose-200"
                >
                  900182008812309 (SOS ჩარლი)
                </button>
              </div>
            </div>

            {lookupResult && lookupResult.source !== 'NOT_FOUND' ? (
              <div className="p-4 rounded-2xl bg-slate-50 border border-slate-200 space-y-2.5">
                <div className="flex items-center justify-between">
                  <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-emerald-100 text-emerald-800">
                    <ShieldCheck className="w-3.5 h-3.5" /> AIRS იდენტიფიცირებულია
                  </span>
                  {lookupResult.isSos && (
                    <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-rose-600 text-white animate-pulse">
                      🚨 იძებნება (SOS)
                    </span>
                  )}
                </div>
                <div className="text-sm font-bold text-slate-900">{lookupResult.data.name}</div>
                <div className="text-xs text-slate-600">{lookupResult.data.breed}</div>
                <div className="grid grid-cols-2 gap-2 pt-2 border-t border-slate-200 text-xs">
                  <div>
                    <span className="text-slate-400 block text-[10px]">საყელოს №</span>
                    <span className="font-mono font-bold text-slate-800">
                      {lookupResult.data.collarNumber}
                    </span>
                  </div>
                  <div>
                    <span className="text-slate-400 block text-[10px]">მესაკუთრე</span>
                    <span className="font-bold text-slate-800">{lookupResult.data.ownerName}</span>
                  </div>
                </div>
                <div className="pt-2 flex items-center justify-between bg-white p-2.5 rounded-xl border border-slate-200">
                  <div className="text-xs">
                    <span className="text-slate-400 block text-[10px]">საკონტაქტო ტელეფონი</span>
                    <span className="font-bold text-teal-700">{lookupResult.data.ownerPhone}</span>
                  </div>
                  <button
                    onClick={() => {
                      addToast('ზარის ინიცირება', `უკავშირდებით: ${lookupResult.data.ownerPhone}`, 'info');
                    }}
                    className="px-3 py-1.5 bg-teal-600 text-white rounded-lg text-xs font-bold flex items-center gap-1"
                  >
                    <Phone className="w-3.5 h-3.5" /> დარეკვა
                  </button>
                </div>
              </div>
            ) : (
              <div className="p-4 rounded-2xl bg-rose-50 border border-rose-200 text-center">
                <p className="text-xs font-bold text-rose-700">
                  ჩანაწერი ვერ მოიძებნა AIRS / NFA ბაზაში
                </p>
                <p className="text-[11px] text-rose-500 mt-0.5">
                  გთხოვთ გადაამოწმოთ 15-ნიშნა კოდის სისწორე.
                </p>
              </div>
            )}
          </div>
        </div>
      </div>
    );
  };

  // --- D. Add Veterinary Record Modal (For Vet Role / Demo) ---
  const VetRecordModal = () => {
    const [recordType, setRecordType] = useState('VACCINE'); // 'VACCINE' | 'MEDICAL'
    const [title, setTitle] = useState('ცოფის გეგმიური რევაქცინაცია (Nobivac)');
    const [batchNumber, setBatchNumber] = useState('NB-2026-998');
    const [clinic, setClinic] = useState('აგრარული უნივერსიტეტის ვეტ-კლინიკა');
    const [vetName, setVetName] = useState('დოქ. ლევან კვარაცხელია');
    const [notes, setNotes] = useState('ცხოველი კლინიკურად ჯანმრთელია. მონაცემი აისახა AIRS რეესტრში.');

    const handleAddVetRecord = (e) => {
      e.preventDefault();
      if (recordType === 'VACCINE') {
        const newVac = {
          id: `vac-${Date.now()}`,
          name: title,
          type: 'Rabies',
          batchNumber,
          dateAdministered: '2026-11-01',
          nextDueDate: '2027-11-01',
          clinic,
          vetName,
          status: 'Active'
        };
        setPets((prev) =>
          prev.map((p) =>
            p.id === selectedPet.id
              ? { ...p, rabiesStatus: 'Active', vaccinations: [newVac, ...p.vaccinations] }
              : p
          )
        );
      } else {
        const newMed = {
          id: `med-${Date.now()}`,
          date: '2026-11-01',
          title,
          category: 'Checkup',
          clinic,
          vetName,
          notes
        };
        setPets((prev) =>
          prev.map((p) =>
            p.id === selectedPet.id
              ? { ...p, medicalRecords: [newMed, ...p.medicalRecords] }
              : p
          )
        );
      }

      setIsVetRecordModalOpen(false);
      addToast(
        'ვეტერინარული ჩანაწერი დაემატა',
        `${selectedPet.name}-ის ციფრული პასპორტი და AIRS ბაზა განახლდა.`,
        'success'
      );
    };

    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/70 backdrop-blur-sm p-4 animate-fadeIn">
        <div className="bg-white w-full max-w-md rounded-3xl shadow-2xl border border-slate-200 overflow-hidden">
          <div className="bg-teal-700 px-5 py-4 text-white flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Stethoscope className="w-5 h-5" />
              <div>
                <h3 className="font-bold text-sm">ვეტ-ჩანაწერის დამატება (AIRS Vet Portal)</h3>
                <p className="text-[11px] text-teal-100">პაციენტი: {selectedPet.name} ({selectedPet.microchipCode})</p>
              </div>
            </div>
            <button onClick={() => setIsVetRecordModalOpen(false)} className="p-1.5 rounded-full bg-white/10">
              <X className="w-4 h-4" />
            </button>
          </div>

          <form onSubmit={handleAddVetRecord} className="p-5 space-y-3.5 text-slate-800">
            <div className="grid grid-cols-2 gap-2 p-1 bg-slate-100 rounded-xl">
              <button
                type="button"
                onClick={() => setRecordType('VACCINE')}
                className={`py-2 rounded-lg text-xs font-bold ${
                  recordType === 'VACCINE' ? 'bg-teal-600 text-white' : 'text-slate-600'
                }`}
              >
                💉 ვაქცინაცია
              </button>
              <button
                type="button"
                onClick={() => setRecordType('MEDICAL')}
                className={`py-2 rounded-lg text-xs font-bold ${
                  recordType === 'MEDICAL' ? 'bg-indigo-600 text-white' : 'text-slate-600'
                }`}
              >
                📋 მანიპულაცია / ვიზიტი
              </button>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1">დასახელება / პრეპარატი *</label>
              <input
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                className="w-full px-3.5 py-2 border border-slate-300 rounded-xl text-sm"
              />
            </div>

            {recordType === 'VACCINE' && (
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">სერია / Batch №</label>
                <input
                  type="text"
                  value={batchNumber}
                  onChange={(e) => setBatchNumber(e.target.value)}
                  className="w-full px-3.5 py-2 border border-slate-300 rounded-xl text-sm font-mono"
                />
              </div>
            )}

            <div className="grid grid-cols-2 gap-2.5">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">ვეტ-კლინიკა</label>
                <input
                  type="text"
                  value={clinic}
                  onChange={(e) => setClinic(e.target.value)}
                  className="w-full px-3 py-2 border border-slate-300 rounded-xl text-xs"
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">პასუხისმგებელი ვეტერინარი</label>
                <input
                  type="text"
                  value={vetName}
                  onChange={(e) => setVetName(e.target.value)}
                  className="w-full px-3 py-2 border border-slate-300 rounded-xl text-xs"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1">კლინიკური შენიშვნა</label>
              <textarea
                rows={2}
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                className="w-full px-3.5 py-2 border border-slate-300 rounded-xl text-xs"
              />
            </div>

            <button
              type="submit"
              className="w-full py-3 bg-teal-600 hover:bg-teal-700 text-white rounded-xl text-xs font-bold shadow-md"
            >
              დადასტურება და AIRS ბაზაში ასახვა
            </button>
          </form>
        </div>
      </div>
    );
  };

  // --- E. QR Code & Official NFA/AIRS Certificate Modal ---
  const QrPassportModal = () => (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/75 backdrop-blur-sm p-4 animate-fadeIn">
      <div className="bg-white w-full max-w-sm rounded-3xl shadow-2xl border border-slate-200 overflow-hidden text-center p-6 space-y-4">
        <div className="flex justify-between items-center">
          <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-emerald-100 text-emerald-800 text-xs font-bold">
            <ShieldCheck className="w-4 h-4" /> AIRS / NFA Verified
          </span>
          <button
            onClick={() => setIsQrModalOpen(false)}
            className="p-1.5 rounded-full bg-slate-100 hover:bg-slate-200 text-slate-600"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        <div className="flex flex-col items-center">
          <img
            src={selectedPet.avatarUrl}
            alt={selectedPet.name}
            className="w-16 h-16 rounded-2xl object-cover border-2 border-teal-500 shadow-md mb-2"
          />
          <h3 className="font-bold text-lg text-slate-900">{selectedPet.name}</h3>
          <p className="text-xs text-slate-500">{selectedPet.breed}</p>
        </div>

        {/* Simulated Crisp Vector QR Code */}
        <div className="mx-auto w-48 h-48 p-4 bg-slate-900 rounded-2xl flex flex-col items-center justify-center relative shadow-inner">
          <div className="grid grid-cols-7 gap-1.5 w-full h-full p-2 bg-white rounded-xl">
            {Array.from({ length: 49 }).map((_, idx) => {
              const isCorner =
                idx < 3 ||
                (idx >= 4 && idx < 7) ||
                idx % 7 === 0 ||
                idx % 3 === 0 ||
                idx === 24;
              return (
                <div
                  key={idx}
                  className={`rounded-sm ${
                    isCorner ? 'bg-slate-900' : idx % 2 === 0 ? 'bg-teal-600' : 'bg-slate-100'
                  }`}
                />
              );
            })}
          </div>
          <div className="absolute bg-white px-2 py-1 rounded-lg shadow-md border border-teal-500 flex items-center gap-1">
            <PawPrint className="w-3.5 h-3.5 text-teal-600" />
            <span className="text-[10px] font-black text-slate-900">AIRS</span>
          </div>
        </div>

        <div className="bg-slate-50 p-3 rounded-2xl border border-slate-200">
          <span className="text-[10px] uppercase tracking-wider text-slate-400 block">
            ISO 11784/11785 15-Digit Microchip
          </span>
          <span className="font-mono text-sm font-black text-slate-900 tracking-widest">
            {selectedPet.microchipCode}
          </span>
          <span className="block text-[11px] text-teal-700 font-semibold mt-0.5">
            საყელოს №: {selectedPet.collarNumber}
          </span>
        </div>

        <p className="text-[11px] text-slate-500 leading-relaxed">
          წარუდგინეთ QR კოდი ვეტერინარს ან ცხოველთა მონიტორინგის სააგენტოს (AMA) ოფიცერს მყისიერი იდენტიფიკაციისთვის.
        </p>
      </div>
    </div>
  );

  // ============================================================================
  // 4. SCREEN VIEWS (HOME, PASSPORT, RADAR, AUTH, DEV SPECS)
  // ============================================================================

  // --- SCREEN 1: HOME DASHBOARD / FEED ---
  const renderHomeScreen = () => (
    <div className="space-y-5 pb-6">
      {/* Welcome & Role Banner */}
      <div className="bg-gradient-to-br from-teal-700 via-teal-600 to-emerald-600 rounded-3xl p-4 text-white shadow-lg shadow-teal-900/10 relative overflow-hidden">
        <div className="flex items-start justify-between relative z-10">
          <div>
            <div className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full bg-white/15 backdrop-blur-md text-[11px] font-semibold mb-2">
              <ShieldCheck className="w-3.5 h-3.5 text-emerald-300" />
              <span>AIRS / სეს ერთიანი ბაზა აქტიურია</span>
            </div>
            <h2 className="text-lg font-extrabold tracking-tight">
              გამარჯობა, {authUser.fullName.split(' ')[0]}! 👋
            </h2>
            <p className="text-xs text-teal-100 mt-0.5">
              როლი: <strong className="text-white">{userRole}</strong> • თბილისის მუნიციპალური არეალი
            </p>
          </div>
          <button
            onClick={() => setIsChipLookupModalOpen(true)}
            className="p-2.5 bg-white/15 hover:bg-white/25 rounded-2xl backdrop-blur-md transition flex flex-col items-center"
            title="15-ნიშნა ჩიპის სკანერი"
          >
            <QrCode className="w-5 h-5" />
            <span className="text-[9px] font-bold mt-0.5">ჩიპ-სკანი</span>
          </button>
        </div>

        {/* Statistics Strip from Status Report (53,656 + 532 registered in Tbilisi) */}
        <div className="grid grid-cols-3 gap-2 mt-4 pt-3 border-t border-white/15 text-center relative z-10">
          <div className="bg-black/10 rounded-xl p-2">
            <span className="text-xs font-black block">54,188</span>
            <span className="text-[10px] text-teal-100">AIRS რეესტრი (თბილისი)</span>
          </div>
          <div className="bg-black/10 rounded-xl p-2">
            <span className="text-xs font-black block">{pets.length} ცხოველი</span>
            <span className="text-[10px] text-teal-100">ჩემი ვეტ-პასპორტი</span>
          </div>
          <div className="bg-rose-500/40 border border-rose-300/30 rounded-xl p-2">
            <span className="text-xs font-black block">{sosAlerts.length} აქტიური</span>
            <span className="text-[10px] text-rose-100">SOS განგაში</span>
          </div>
        </div>
      </div>

      {/* LIVE EMERGENCY SOS ALERT CAROUSEL */}
      <div>
        <div className="flex items-center justify-between mb-2.5">
          <div className="flex items-center gap-2">
            <span className="w-2.5 h-2.5 rounded-full bg-rose-600 animate-ping" />
            <h3 className="text-sm font-extrabold text-slate-900">
              სასწრაფო SOS განგაში თბილისში
            </h3>
          </div>
          <button
            onClick={() => setActiveTab('radar')}
            className="text-xs font-bold text-rose-600 hover:underline flex items-center gap-0.5"
          >
            რუკაზე ნახვა <ChevronRight className="w-3.5 h-3.5" />
          </button>
        </div>

        <div className="space-y-2.5">
          {sosAlerts.slice(0, 2).map((alert) => (
            <div
              key={alert.id}
              className={`p-3.5 rounded-2xl border transition shadow-sm ${
                alert.type === 'LOST'
                  ? 'bg-rose-50/70 border-rose-200'
                  : 'bg-teal-50/70 border-teal-200'
              }`}
            >
              <div className="flex gap-3">
                <img
                  src={alert.avatarUrl}
                  alt={alert.petName}
                  className="w-16 h-16 rounded-2xl object-cover border border-white shadow-sm flex-shrink-0"
                />
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between gap-1">
                    <span
                      className={`px-2 py-0.5 rounded-full text-[10px] font-extrabold ${
                        alert.type === 'LOST'
                          ? 'bg-rose-600 text-white'
                          : 'bg-teal-600 text-white'
                      }`}
                    >
                      {alert.type === 'LOST' ? '🚨 დაიკარგა (LOST)' : '🐾 ნაპოვნია (FOUND)'}
                    </span>
                    <span className="text-[11px] font-semibold text-slate-500 flex items-center gap-1">
                      <Clock className="w-3 h-3" /> {alert.lastSeenTime}
                    </span>
                  </div>

                  <h4 className="font-bold text-sm text-slate-900 mt-1 truncate">
                    {alert.petName} — <span className="font-medium text-slate-600 text-xs">{alert.breed}</span>
                  </h4>

                  <div className="flex items-center gap-1 text-xs text-slate-700 mt-0.5">
                    <MapPin className="w-3.5 h-3.5 text-rose-600 flex-shrink-0" />
                    <span className="truncate font-medium">{alert.neighborhood}</span>
                    <span className="text-[10px] bg-white px-1.5 py-0.5 rounded border border-slate-200 font-bold text-slate-600">
                      {alert.distanceKm}
                    </span>
                  </div>
                </div>
              </div>

              {/* Microchip & Action Bar */}
              <div className="mt-3 pt-2.5 border-t border-slate-200/80 flex items-center justify-between gap-2">
                <div className="text-[11px] font-mono text-slate-600 truncate">
                  <span className="font-bold text-slate-800">ჩიპი:</span> {alert.microchipCode} •{' '}
                  <span className="text-indigo-700 font-bold">{alert.collarNumber}</span>
                </div>
                <button
                  onClick={() => setActiveSightingAlert(alert)}
                  className="px-3 py-1.5 rounded-xl bg-slate-900 hover:bg-slate-800 text-white text-[11px] font-bold flex items-center gap-1 flex-shrink-0 shadow-sm"
                >
                  <Phone className="w-3 h-3" /> დავაფიქსირე / კონტაქტი
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* QUICK ACTIONS GRID (4 Pillars) */}
      <div>
        <h3 className="text-sm font-extrabold text-slate-900 mb-2.5">სწრაფი მოქმედებები</h3>
        <div className="grid grid-cols-2 gap-2.5">
          <button
            onClick={() => setIsSosModalOpen(true)}
            className="p-3.5 rounded-2xl bg-rose-600 hover:bg-rose-700 text-white text-left transition shadow-md shadow-rose-600/20 flex flex-col justify-between"
          >
            <div className="w-9 h-9 rounded-xl bg-white/20 flex items-center justify-center mb-2">
              <AlertTriangle className="w-5 h-5 text-white" />
            </div>
            <div>
              <span className="text-xs font-extrabold block">SOS დაკარგვის გამოცხადება</span>
              <span className="text-[10px] text-rose-100">მყისიერი Push-განგაში</span>
            </div>
          </button>

          <button
            onClick={() => setIsChipLookupModalOpen(true)}
            className="p-3.5 rounded-2xl bg-indigo-600 hover:bg-indigo-700 text-white text-left transition shadow-md shadow-indigo-600/20 flex flex-col justify-between"
          >
            <div className="w-9 h-9 rounded-xl bg-white/20 flex items-center justify-center mb-2">
              <Search className="w-5 h-5 text-white" />
            </div>
            <div>
              <span className="text-xs font-extrabold block">AIRS ჩიპის შემოწმება</span>
              <span className="text-[10px] text-indigo-100">15-ნიშნა ISO იდენტიფიკაცია</span>
            </div>
          </button>

          <button
            onClick={() => {
              setEditingPet(null);
              setIsPetCrudModalOpen(true);
            }}
            className="p-3.5 rounded-2xl bg-white hover:bg-teal-50/50 border border-slate-200 text-slate-800 text-left transition shadow-sm flex flex-col justify-between"
          >
            <div className="w-9 h-9 rounded-xl bg-teal-100 text-teal-700 flex items-center justify-center mb-2">
              <Plus className="w-5 h-5" />
            </div>
            <div>
              <span className="text-xs font-extrabold block">ახალი ცხოველის დამატება</span>
              <span className="text-[10px] text-slate-500">CRUD & AIRS რეგისტრაცია</span>
            </div>
          </button>

          <button
            onClick={() => {
              setIsVetRecordModalOpen(true);
            }}
            className="p-3.5 rounded-2xl bg-white hover:bg-emerald-50/50 border border-slate-200 text-slate-800 text-left transition shadow-sm flex flex-col justify-between"
          >
            <div className="w-9 h-9 rounded-xl bg-emerald-100 text-emerald-700 flex items-center justify-center mb-2">
              <Stethoscope className="w-5 h-5" />
            </div>
            <div>
              <span className="text-xs font-extrabold block">ვეტ-ჩანაწერი / ვიზიტი</span>
              <span className="text-[10px] text-slate-500">ვაქცინაცია და ისტორია</span>
            </div>
          </button>
        </div>
      </div>

      {/* MY PETS SECTION ("ჩემი ცხოველები") */}
      <div>
        <div className="flex items-center justify-between mb-2.5">
          <div>
            <h3 className="text-sm font-extrabold text-slate-900">ჩემი ცხოველები ({pets.length})</h3>
            <p className="text-[11px] text-slate-500">დააჭირეთ ბარათს ციფრული ვეტ-პასპორტის გასახსნელად</p>
          </div>
          <button
            onClick={() => {
              setEditingPet(null);
              setIsPetCrudModalOpen(true);
            }}
            className="px-3 py-1.5 rounded-xl bg-teal-50 hover:bg-teal-100 text-teal-700 text-xs font-bold flex items-center gap-1"
          >
            <Plus className="w-3.5 h-3.5" /> დამატება
          </button>
        </div>

        <div className="space-y-3">
          {pets.map((pet) => (
            <div
              key={pet.id}
              onClick={() => {
                setSelectedPetId(pet.id);
                setActiveTab('passport');
              }}
              className="group bg-white rounded-2xl p-4 border border-slate-200 hover:border-teal-500 shadow-sm hover:shadow-md transition cursor-pointer relative overflow-hidden"
            >
              <div className="flex items-center gap-3.5">
                <div className="relative">
                  <img
                    src={pet.avatarUrl}
                    alt={pet.name}
                    className="w-16 h-16 rounded-2xl object-cover border-2 border-teal-500/30"
                  />
                  {pet.airsVerified && (
                    <div
                      className="absolute -bottom-1 -right-1 bg-emerald-500 text-white p-1 rounded-full shadow"
                      title="AIRS სინქრონიზებული"
                    >
                      <ShieldCheck className="w-3.5 h-3.5" />
                    </div>
                  )}
                </div>

                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between">
                    <h4 className="font-extrabold text-base text-slate-900 group-hover:text-teal-700 transition truncate">
                      {pet.name}
                    </h4>
                    <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-50 text-emerald-700 border border-emerald-200">
                      <CheckCircle2 className="w-3 h-3" /> AIRS სინქრონიზებული
                    </span>
                  </div>

                  <p className="text-xs text-slate-600 truncate mt-0.5">{pet.breed}</p>

                  <div className="flex flex-wrap items-center gap-1.5 mt-2">
                    <span className="px-2 py-0.5 bg-slate-100 text-slate-700 rounded-md text-[10px] font-bold">
                      {pet.petTypeLabel} • {pet.genderLabel}
                    </span>
                    <span className="px-2 py-0.5 bg-teal-50 text-teal-800 rounded-md text-[10px] font-mono font-bold">
                      ჩიპი: {pet.microchipCode}
                    </span>
                    <span className="px-2 py-0.5 bg-indigo-50 text-indigo-700 rounded-md text-[10px] font-mono font-bold">
                      {pet.collarNumber}
                    </span>
                  </div>
                </div>

                <ChevronRight className="w-5 h-5 text-slate-400 group-hover:text-teal-600 group-hover:translate-x-0.5 transition" />
              </div>

              {/* CRUD Action Strip */}
              <div
                onClick={(e) => e.stopPropagation()}
                className="mt-3 pt-2.5 border-t border-slate-100 flex items-center justify-between text-xs"
              >
                <div className="flex items-center gap-2">
                  <span
                    className={`inline-flex items-center gap-1 text-[11px] font-bold ${
                      pet.rabiesStatus === 'Active' ? 'text-emerald-600' : 'text-amber-600'
                    }`}
                  >
                    <Syringe className="w-3.5 h-3.5" /> ცოფის ვაქცინა: {pet.rabiesStatus === 'Active' ? 'აქტიური' : 'საჭიროებს განახლებას'}
                  </span>
                </div>
                <div className="flex items-center gap-1.5">
                  <button
                    onClick={() => {
                      setEditingPet(pet);
                      setIsPetCrudModalOpen(true);
                    }}
                    className="p-1.5 rounded-lg text-slate-500 hover:text-teal-700 hover:bg-slate-100"
                    title="რედაქტირება"
                  >
                    <Edit3 className="w-3.5 h-3.5" />
                  </button>
                  <button
                    onClick={() => setPetToDelete(pet)}
                    className="p-1.5 rounded-lg text-slate-500 hover:text-rose-600 hover:bg-rose-50"
                    title="წაშლა"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );

  // --- SCREEN 2: DIGITAL VET PASSPORT (AIRS / NFA Synchronized) ---
  const renderPassportScreen = () => {
    if (!selectedPet) return null;

    return (
      <div className="space-y-4 pb-6">
        {/* Pet Switcher Pills */}
        <div className="flex items-center gap-2 overflow-x-auto pb-1">
          {pets.map((p) => (
            <button
              key={p.id}
              onClick={() => setSelectedPetId(p.id)}
              className={`px-3.5 py-2 rounded-2xl text-xs font-bold flex items-center gap-2 whitespace-nowrap transition ${
                p.id === selectedPet.id
                  ? 'bg-teal-600 text-white shadow-sm'
                  : 'bg-white text-slate-700 border border-slate-200 hover:bg-slate-50'
              }`}
            >
              <img src={p.avatarUrl} alt={p.name} className="w-5 h-5 rounded-full object-cover" />
              {p.name}
            </button>
          ))}
        </div>

        {/* Official Passport Header Card */}
        <div className="bg-gradient-to-br from-slate-900 via-teal-950 to-slate-900 text-white rounded-3xl p-5 shadow-xl relative overflow-hidden border border-teal-500/30">
          <div className="flex items-center justify-between border-b border-white/10 pb-3 mb-4">
            <div className="flex items-center gap-2">
              <div className="w-8 h-8 rounded-xl bg-emerald-500/20 border border-emerald-400/40 flex items-center justify-center">
                <ShieldCheck className="w-5 h-5 text-emerald-400" />
              </div>
              <div>
                <span className="text-[10px] uppercase tracking-widest text-teal-300 font-bold block">
                  საქართველოს ერთიანი ელექტრონული რეესტრი
                </span>
                <h2 className="text-xs font-extrabold text-white">
                  ციფრული ვეტერინარული პასპორტი • AIRS / NFA
                </h2>
              </div>
            </div>
            <button
              onClick={() => setIsQrModalOpen(true)}
              className="px-3 py-1.5 rounded-xl bg-white/10 hover:bg-white/20 text-xs font-bold flex items-center gap-1.5 backdrop-blur-md border border-white/15"
            >
              <QrCode className="w-4 h-4 text-teal-300" /> QR კოდი
            </button>
          </div>

          {/* Pet Identity Row */}
          <div className="flex items-center gap-4">
            <img
              src={selectedPet.avatarUrl}
              alt={selectedPet.name}
              className="w-20 h-20 rounded-2xl object-cover border-2 border-teal-400 shadow-lg"
            />
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2">
                <h3 className="text-xl font-black truncate">{selectedPet.name}</h3>
                <span className="px-2 py-0.5 rounded-md bg-teal-500/20 border border-teal-400/30 text-teal-200 text-[10px] font-bold">
                  {selectedPet.petTypeLabel}
                </span>
              </div>
              <p className="text-xs text-slate-300 truncate">{selectedPet.breed}</p>
              <div className="grid grid-cols-3 gap-2 mt-2.5 text-[11px]">
                <div className="bg-white/5 px-2.5 py-1 rounded-lg">
                  <span className="text-slate-400 block text-[9px]">სქესი</span>
                  <span className="font-bold">{selectedPet.genderLabel}</span>
                </div>
                <div className="bg-white/5 px-2.5 py-1 rounded-lg">
                  <span className="text-slate-400 block text-[9px]">ასაკი</span>
                  <span className="font-bold">{selectedPet.age}</span>
                </div>
                <div className="bg-white/5 px-2.5 py-1 rounded-lg">
                  <span className="text-slate-400 block text-[9px]">წონა</span>
                  <span className="font-bold">{selectedPet.weight}</span>
                </div>
              </div>
            </div>
          </div>

          {/* 15-Digit ISO Microchip & Collar Card */}
          <div className="mt-4 p-3.5 rounded-2xl bg-white/10 backdrop-blur-md border border-white/15 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
            <div>
              <span className="text-[10px] uppercase tracking-wider text-teal-200 font-semibold block">
                15-ნიშნა ISO მიკროჩიპის კოდი (Unique ID)
              </span>
              <div className="flex items-center gap-2 mt-0.5">
                <span className="font-mono text-base font-black tracking-widest text-white">
                  {selectedPet.microchipCode}
                </span>
                <button
                  onClick={() => handleCopy(selectedPet.microchipCode, 'მიკროჩიპის კოდი')}
                  className="p-1 rounded-lg bg-white/10 hover:bg-white/20 text-teal-200"
                  title="კოდის კოპირება"
                >
                  {copiedText === selectedPet.microchipCode ? (
                    <Check className="w-3.5 h-3.5 text-emerald-400" />
                  ) : (
                    <Copy className="w-3.5 h-3.5" />
                  )}
                </button>
              </div>
            </div>

            <div className="sm:text-right border-t sm:border-t-0 pt-2 sm:pt-0 border-white/10">
              <span className="text-[10px] uppercase tracking-wider text-indigo-200 font-semibold block">
                თბილისის AMA საყელოს №
              </span>
              <span className="font-mono text-sm font-bold text-amber-300">
                {selectedPet.collarNumber}
              </span>
            </div>
          </div>

          {/* Official Status Badges */}
          <div className="mt-3 flex flex-wrap gap-2">
            <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[11px] font-bold bg-emerald-500/20 text-emerald-300 border border-emerald-400/30">
              <CheckCircle2 className="w-3.5 h-3.5" />
              {selectedPet.isSterilized ? 'სტერილიზებული / კასტრირებული' : 'არასტერილიზებული'}
            </span>
            <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[11px] font-bold bg-teal-500/20 text-teal-200 border border-teal-400/30">
              <Syringe className="w-3.5 h-3.5" />
              ცოფის ვაქცინაცია: ვალიდურია ({selectedPet.rabiesValidUntil}-მდე)
            </span>
          </div>
        </div>

        {/* Visual Characteristics & Ownership Data (Tbilisi AMA requirement) */}
        <div className="bg-white rounded-2xl p-4 border border-slate-200 shadow-sm space-y-3">
          <div className="flex items-center justify-between">
            <h4 className="text-xs font-extrabold uppercase tracking-wider text-slate-500 flex items-center gap-1.5">
              <FileText className="w-4 h-4 text-teal-600" />
              ვიზუალური მახასიათებლები და მესაკუთრე
            </h4>
            <button
              onClick={() => {
                setEditingPet(selectedPet);
                setIsPetCrudModalOpen(true);
              }}
              className="text-xs font-bold text-teal-600 hover:underline flex items-center gap-1"
            >
              <Edit3 className="w-3.5 h-3.5" /> შეცვლა
            </button>
          </div>

          <p className="text-xs text-slate-700 bg-slate-50 p-3 rounded-xl border border-slate-100 leading-relaxed">
            {selectedPet.visualCharacteristics}
          </p>

          <div className="grid grid-cols-2 gap-3 pt-1 text-xs">
            <div className="p-2.5 rounded-xl bg-slate-50 border border-slate-100">
              <span className="text-[10px] text-slate-400 block">მესაკუთრე (Owner)</span>
              <span className="font-bold text-slate-800">{selectedPet.ownerName}</span>
              <span className="block text-[10px] font-mono text-slate-500 mt-0.5">
                პ/ნ: {selectedPet.ownerPersonalId}
              </span>
            </div>
            <div className="p-2.5 rounded-xl bg-slate-50 border border-slate-100">
              <span className="text-[10px] text-slate-400 block">საკონტაქტო ნომერი</span>
              <span className="font-bold text-teal-700">{selectedPet.ownerPhone}</span>
              <span className="block text-[10px] text-emerald-600 font-semibold mt-0.5">
                ✓ ვერიფიცირებულია JWT-ით
              </span>
            </div>
          </div>
        </div>

        {/* Vaccination History & Rabies Timeline */}
        <div className="bg-white rounded-2xl p-4 border border-slate-200 shadow-sm space-y-3">
          <div className="flex items-center justify-between">
            <div>
              <h4 className="text-sm font-extrabold text-slate-900 flex items-center gap-1.5">
                <Syringe className="w-4 h-4 text-teal-600" />
                ვაქცინაციის ისტორია (ცოფი და კომპლექსური)
              </h4>
              <p className="text-[11px] text-slate-500">სინქრონიზებულია ვეტერინარული კლინიკების მიერ</p>
            </div>
            <button
              onClick={() => setIsVetRecordModalOpen(true)}
              className="px-2.5 py-1.5 rounded-xl bg-teal-50 hover:bg-teal-100 text-teal-700 text-xs font-bold flex items-center gap-1"
            >
              <Plus className="w-3.5 h-3.5" /> დამატება
            </button>
          </div>

          <div className="space-y-2.5">
            {selectedPet.vaccinations.map((vac) => (
              <div
                key={vac.id}
                className="p-3 rounded-xl bg-slate-50 border border-slate-200/80 flex flex-col gap-1.5"
              >
                <div className="flex items-start justify-between gap-2">
                  <div>
                    <span className="font-bold text-xs text-slate-900 block">{vac.name}</span>
                    <span className="text-[11px] text-slate-500">
                      {vac.clinic} • {vac.vetName}
                    </span>
                  </div>
                  <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-100 text-emerald-800 whitespace-nowrap">
                    ✓ აქტიური
                  </span>
                </div>
                <div className="flex items-center justify-between text-[11px] pt-1.5 border-t border-slate-200/60 text-slate-600">
                  <span className="font-mono">Batch: {vac.batchNumber}</span>
                  <span>ჩატარდა: {vac.dateAdministered}</span>
                  <span className="font-bold text-teal-700">მომდევნო: {vac.nextDueDate}</span>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Medical Manipulations & Vet Records Log */}
        <div className="bg-white rounded-2xl p-4 border border-slate-200 shadow-sm space-y-3">
          <div className="flex items-center justify-between">
            <h4 className="text-sm font-extrabold text-slate-900 flex items-center gap-1.5">
              <Activity className="w-4 h-4 text-indigo-600" />
              ვეტერინარული მანიპულაციები და ჩანაწერები
            </h4>
            {(userRole === 'Vet' || userRole === 'Owner') && (
              <button
                onClick={() => setIsVetRecordModalOpen(true)}
                className="text-xs font-bold text-indigo-600 hover:underline"
              >
                + ახალი ჩანაწერი
              </button>
            )}
          </div>

          <div className="space-y-2.5">
            {selectedPet.medicalRecords.map((rec) => (
              <div key={rec.id} className="p-3 rounded-xl border border-slate-200 bg-white space-y-1">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-bold text-slate-900">{rec.title}</span>
                  <span className="text-[11px] font-mono text-slate-500">{rec.date}</span>
                </div>
                <p className="text-xs text-slate-600 leading-relaxed">{rec.notes}</p>
                <div className="text-[10px] text-indigo-600 font-semibold pt-1">
                  🏥 {rec.clinic} — {rec.vetName}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  };

  // --- SCREEN 3: LOST & FOUND INTERACTIVE TBILISI RADAR & FEED ---
  const renderRadarScreen = () => {
    const filteredAlerts = sosAlerts.filter((a) =>
      radarFilter === 'ALL' ? true : a.type === radarFilter
    );

    return (
      <div className="space-y-4 pb-6">
        {/* Top Radar Header & SOS CTA */}
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-base font-extrabold text-slate-900 flex items-center gap-1.5">
              <Navigation className="w-4 h-4 text-rose-600" />
              თბილისის SOS რადარი (Live GPS)
            </h2>
            <p className="text-xs text-slate-500">დაკარგული და ნაპოვნი ცხოველების მონიტორინგი</p>
          </div>
          <button
            onClick={() => setIsSosModalOpen(true)}
            className="px-3.5 py-2 rounded-xl bg-rose-600 hover:bg-rose-700 text-white text-xs font-bold shadow-md shadow-rose-600/20 flex items-center gap-1.5"
          >
            <AlertTriangle className="w-4 h-4" /> + SOS განგაში
          </button>
        </div>

        {/* Simulated Interactive Tbilisi Map Canvas */}
        <div className="relative h-64 w-full rounded-3xl overflow-hidden border-2 border-slate-200 shadow-inner bg-slate-900">
          {/* Stylized Vector Map Grid & Mtkvari River */}
          <svg className="w-full h-full opacity-35" viewBox="0 0 400 260" fill="none">
            <rect width="400" height="260" fill="#0F172A" />
            {/* Grid Lines */}
            <path d="M0 50 H400 M0 100 H400 M0 150 H400 M0 200 H400" stroke="#334155" strokeWidth="1" />
            <path d="M80 0 V260 M160 0 V260 M240 0 V260 M320 0 V260" stroke="#334155" strokeWidth="1" />
            {/* Simulated Mtkvari River curving through Tbilisi */}
            <path
              d="M 10 20 Q 140 70, 210 130 T 390 230"
              stroke="#0D9488"
              strokeWidth="18"
              strokeLinecap="round"
              fill="none"
              opacity="0.5"
            />
            {/* Major Roads */}
            <path d="M 40 220 L 350 40" stroke="#64748B" strokeWidth="3" strokeDasharray="6 4" />
          </svg>

          {/* Neighborhood Labels */}
          <div className="absolute top-3 left-3 bg-slate-900/80 backdrop-blur-md px-2.5 py-1 rounded-lg border border-slate-700 text-[10px] font-bold text-slate-300">
            📍 საბურთალო / ლისი
          </div>
          <div className="absolute bottom-14 left-6 bg-slate-900/80 backdrop-blur-md px-2.5 py-1 rounded-lg border border-slate-700 text-[10px] font-bold text-slate-300">
            🌳 ვაკის პარკი
          </div>
          <div className="absolute bottom-12 right-4 bg-slate-900/80 backdrop-blur-md px-2.5 py-1 rounded-lg border border-slate-700 text-[10px] font-bold text-slate-300">
            🏛️ მთაწმინდა / რუსთაველი
          </div>

          {/* Interactive Pulsing Pins */}
          {filteredAlerts.map((alert) => {
            const isSelected = selectedPinAlert?.id === alert.id;
            return (
              <button
                key={alert.id}
                onClick={() => setSelectedPinAlert(alert)}
                style={{ left: `${alert.coords.x}%`, top: `${alert.coords.y}%` }}
                className="absolute -translate-x-1/2 -translate-y-1/2 group focus:outline-none"
              >
                <span
                  className={`absolute -inset-2 rounded-full animate-ping opacity-75 ${
                    alert.type === 'LOST' ? 'bg-rose-500' : 'bg-teal-400'
                  }`}
                />
                <div
                  className={`relative flex items-center gap-1.5 px-2.5 py-1 rounded-full font-bold text-[11px] shadow-lg transition transform ${
                    isSelected ? 'scale-110 ring-2 ring-white' : 'hover:scale-105'
                  } ${
                    alert.type === 'LOST'
                      ? 'bg-rose-600 text-white'
                      : 'bg-teal-600 text-white'
                  }`}
                >
                  <PawPrint className="w-3 h-3" />
                  <span>{alert.petName.split(' ')[0]}</span>
                </div>
              </button>
            );
          })}

          {/* Selected Pin Mini-Overlay at Bottom of Map */}
          {selectedPinAlert && (
            <div className="absolute bottom-2.5 inset-x-2.5 bg-white/95 backdrop-blur-md p-2.5 rounded-2xl shadow-lg border border-slate-200 flex items-center justify-between gap-2">
              <div className="flex items-center gap-2.5 min-w-0">
                <img
                  src={selectedPinAlert.avatarUrl}
                  alt={selectedPinAlert.petName}
                  className="w-10 h-10 rounded-xl object-cover flex-shrink-0"
                />
                <div className="min-w-0">
                  <div className="flex items-center gap-1.5">
                    <span className="text-xs font-extrabold text-slate-900 truncate">
                      {selectedPinAlert.petName}
                    </span>
                    <span className="text-[10px] font-mono bg-slate-100 px-1.5 py-0.5 rounded text-slate-700">
                      {selectedPinAlert.collarNumber}
                    </span>
                  </div>
                  <p className="text-[11px] text-slate-600 truncate">{selectedPinAlert.neighborhood}</p>
                </div>
              </div>
              <button
                onClick={() => setActiveSightingAlert(selectedPinAlert)}
                className="px-3 py-1.5 bg-rose-600 text-white rounded-xl text-[11px] font-bold whitespace-nowrap"
              >
                დეტალები
              </button>
            </div>
          )}
        </div>

        {/* Filter Pills */}
        <div className="flex items-center gap-2">
          {[
            { id: 'ALL', label: `ყველა (${sosAlerts.length})` },
            {
              id: 'LOST',
              label: `🚨 დაკარგული (${sosAlerts.filter((x) => x.type === 'LOST').length})`
            },
            {
              id: 'FOUND',
              label: `🐾 ნაპოვნი (${sosAlerts.filter((x) => x.type === 'FOUND').length})`
            }
          ].map((tab) => (
            <button
              key={tab.id}
              onClick={() => setRadarFilter(tab.id)}
              className={`px-3.5 py-1.5 rounded-xl text-xs font-bold transition ${
                radarFilter === tab.id
                  ? 'bg-slate-900 text-white'
                  : 'bg-white text-slate-600 border border-slate-200 hover:bg-slate-50'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* Detailed Alert Feed Cards */}
        <div className="space-y-3">
          {filteredAlerts.map((alert) => (
            <div
              key={alert.id}
              className="bg-white rounded-2xl p-4 border border-slate-200 shadow-sm space-y-3"
            >
              <div className="flex items-start gap-3">
                <img
                  src={alert.avatarUrl}
                  alt={alert.petName}
                  className="w-16 h-16 rounded-2xl object-cover border border-slate-200"
                />
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between">
                    <span
                      className={`px-2.5 py-0.5 rounded-full text-[10px] font-extrabold ${
                        alert.type === 'LOST'
                          ? 'bg-rose-100 text-rose-700'
                          : 'bg-teal-100 text-teal-800'
                      }`}
                    >
                      {alert.type === 'LOST' ? 'დაკარგულია • იძებნება' : 'ნაპოვნია • ეძებს პატრონს'}
                    </span>
                    {alert.reward && (
                      <span className="px-2 py-0.5 rounded-full text-[10px] font-extrabold bg-amber-100 text-amber-800">
                        🏆 ჯილდო: {alert.reward}
                      </span>
                    )}
                  </div>
                  <h4 className="font-extrabold text-sm text-slate-900 mt-1">{alert.petName}</h4>
                  <p className="text-xs text-slate-500">{alert.breed}</p>
                </div>
              </div>

              <p className="text-xs text-slate-700 bg-slate-50 p-2.5 rounded-xl border border-slate-100">
                {alert.description}
              </p>

              <div className="grid grid-cols-2 gap-2 text-[11px] bg-slate-50/60 p-2.5 rounded-xl">
                <div>
                  <span className="text-slate-400 block text-[10px]">15-ნიშნა მიკროჩიპი</span>
                  <span className="font-mono font-bold text-slate-800">{alert.microchipCode}</span>
                </div>
                <div>
                  <span className="text-slate-400 block text-[10px]">ლოკაცია ({alert.distanceKm})</span>
                  <span className="font-bold text-slate-800">{alert.neighborhood}</span>
                </div>
              </div>

              <div className="flex items-center justify-between pt-1">
                <span className="text-[11px] text-slate-500">
                  👁️ დაფიქსირებულია <strong>{alert.sightingsCount}-ჯერ</strong> • {alert.lastSeenTime}
                </span>
                <div className="flex gap-2">
                  <button
                    onClick={() => setActiveSightingAlert(alert)}
                    className="px-3.5 py-2 rounded-xl bg-rose-600 hover:bg-rose-700 text-white text-xs font-bold flex items-center gap-1.5 shadow-sm"
                  >
                    <MapPin className="w-3.5 h-3.5" /> დავაფიქსირე / კონტაქტი
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  };

  // --- SCREEN 4: AUTHENTICATION & ONBOARDING (JWT + PBKDF2 + Role Selection) ---
  const [authMode, setAuthMode] = useState('login'); // 'login' | 'register'
  const [formEmail, setFormEmail] = useState('gogita.jgarkava@happypet.ge');
  const [formPassword, setFormPassword] = useState('••••••••••••');
  const [formFullName, setFormFullName] = useState('გოგიტა ჯღარკავა');
  const [formPersonalId, setFormPersonalId] = useState('01024085912');
  const [showPassword, setShowPassword] = useState(false);

  const handleAuthSubmit = (e) => {
    e.preventDefault();
    if (authMode === 'register' && !/^\d{11}$/.test(formPersonalId)) {
      addToast(
        'პირადი ნომრის ვალიდაცია',
        'საქართველოს მოქალაქის პირადი ნომერი უნდა შეიცავდეს ზუსტად 11 ციფრს (Unique Index).',
        'error'
      );
      return;
    }
    setAuthUser((prev) => ({
      ...prev,
      email: formEmail,
      fullName: formFullName,
      personalNumber: formPersonalId,
      role: userRole
    }));
    addToast(
      'ავტორიზაცია წარმატებულია (JWT HS256)',
      `გენერირებულია 7-დღიანი Bearer ტოკენი როლით: [${userRole}]`,
      'success'
    );
    setActiveTab('home');
  };

  const renderAuthScreen = () => (
    <div className="space-y-5 pb-6">
      <div className="text-center pt-2">
        <div className="w-14 h-14 rounded-2xl bg-teal-600 text-white flex items-center justify-center mx-auto shadow-lg shadow-teal-600/30 mb-3">
          <PawPrint className="w-8 h-8" />
        </div>
        <h2 className="text-xl font-black text-slate-900">HappyPet Georgia</h2>
        <p className="text-xs text-slate-500 mt-0.5">
          ერთიანი ციფრული ვეტ-პასპორტი და AIRS იდენტიფიკაციის სისტემა
        </p>
      </div>

      {/* Mode Switcher */}
      <div className="grid grid-cols-2 gap-2 p-1 bg-slate-200/80 rounded-2xl">
        <button
          type="button"
          onClick={() => setAuthMode('login')}
          className={`py-2.5 rounded-xl text-xs font-extrabold transition ${
            authMode === 'login' ? 'bg-white text-slate-900 shadow-sm' : 'text-slate-600'
          }`}
        >
          შესვლა (JWT Login)
        </button>
        <button
          type="button"
          onClick={() => setAuthMode('register')}
          className={`py-2.5 rounded-xl text-xs font-extrabold transition ${
            authMode === 'register' ? 'bg-white text-slate-900 shadow-sm' : 'text-slate-600'
          }`}
        >
          რეგისტრაცია (Onboarding)
        </button>
      </div>

      {/* Role Selector (Matches HappyPet.Domain Role Enum) */}
      <div>
        <label className="block text-xs font-extrabold text-slate-700 mb-2">
          აირჩიეთ მომხმარებლის როლი (RBAC Role Enum)
        </label>
        <div className="grid grid-cols-3 gap-2">
          {[
            { id: 'Owner', title: 'მესაკუთრე', sub: 'Pet Owner', icon: User },
            { id: 'Vet', title: 'ვეტ-კლინიკა', sub: 'Veterinarian', icon: Stethoscope },
            { id: 'Shelter', title: 'თავშესაფარი', sub: 'Shelter / AMA', icon: Building2 }
          ].map((r) => {
            const Icon = r.icon;
            const active = userRole === r.id;
            return (
              <button
                key={r.id}
                type="button"
                onClick={() => setUserRole(r.id)}
                className={`p-3 rounded-2xl border text-center transition flex flex-col items-center ${
                  active
                    ? 'bg-teal-50 border-teal-600 text-teal-900 ring-2 ring-teal-500/20'
                    : 'bg-white border-slate-200 text-slate-600 hover:bg-slate-50'
                }`}
              >
                <Icon className={`w-5 h-5 mb-1 ${active ? 'text-teal-600' : 'text-slate-400'}`} />
                <span className="text-xs font-extrabold block">{r.title}</span>
                <span className="text-[10px] text-slate-400">{r.sub}</span>
              </button>
            );
          })}
        </div>
      </div>

      {/* Auth Form */}
      <form
        onSubmit={handleAuthSubmit}
        className="bg-white p-5 rounded-3xl border border-slate-200 shadow-sm space-y-3.5"
      >
        {authMode === 'register' && (
          <>
            <div>
              <label className="block text-xs font-bold text-slate-700 mb-1">
                სახელი და გვარი / ორგანიზაცია *
              </label>
              <input
                type="text"
                value={formFullName}
                onChange={(e) => setFormFullName(e.target.value)}
                className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm"
              />
            </div>

            <div>
              <div className="flex justify-between mb-1">
                <label className="text-xs font-bold text-slate-700">
                  პირადი ნომერი (11-ნიშნა Unique Index) *
                </label>
                <span className="text-[11px] font-mono font-bold text-teal-700">
                  {formPersonalId.length}/11
                </span>
              </div>
              <input
                type="text"
                maxLength={11}
                value={formPersonalId}
                onChange={(e) => setFormPersonalId(e.target.value.replace(/\D/g, ''))}
                placeholder="01024085912"
                className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 font-mono text-sm"
              />
            </div>
          </>
        )}

        <div>
          <label className="block text-xs font-bold text-slate-700 mb-1">ელ-ფოსტა (Email) *</label>
          <div className="relative">
            <Mail className="w-4 h-4 text-slate-400 absolute left-3.5 top-3" />
            <input
              type="email"
              value={formEmail}
              onChange={(e) => setFormEmail(e.target.value)}
              className="w-full pl-10 pr-3.5 py-2.5 rounded-xl border border-slate-300 text-sm"
            />
          </div>
        </div>

        <div>
          <label className="block text-xs font-bold text-slate-700 mb-1">
            პაროლი (PBKDF2-SHA256 ჰეშირება) *
          </label>
          <div className="relative">
            <Lock className="w-4 h-4 text-slate-400 absolute left-3.5 top-3" />
            <input
              type={showPassword ? 'text' : 'password'}
              value={formPassword}
              onChange={(e) => setFormPassword(e.target.value)}
              className="w-full pl-10 pr-10 py-2.5 rounded-xl border border-slate-300 text-sm"
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute right-3 top-2.5 text-slate-400 hover:text-slate-600"
            >
              {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
            </button>
          </div>
        </div>

        <button
          type="submit"
          className="w-full py-3 rounded-xl bg-teal-600 hover:bg-teal-700 text-white text-xs font-extrabold shadow-lg shadow-teal-600/20 transition"
        >
          {authMode === 'login'
            ? 'ავტორიზაცია და JWT ტოკენის მიღება'
            : 'ანგარიშის შექმნა და AIRS ვერიფიკაცია'}
        </button>

        <div className="pt-2 border-t border-slate-100 text-[11px] text-slate-500 flex items-center justify-between">
          <span>🔒 PBKDF2 (100,000 iterations + Salt)</span>
          <span className="font-mono text-teal-700">JWT HS256 (7d)</span>
        </div>
      </form>
    </div>
  );

  // ============================================================================
  // 5. DEVELOPER HANDOFF & ARCHITECTURE SPECIFICATION VIEW (For Irakli & Luka)
  // ============================================================================
  const renderDeveloperHandoffSpecs = () => (
    <div className="max-w-5xl mx-auto p-4 sm:p-6 space-y-6 text-slate-800">
      {/* Header Banner */}
      <div className="bg-slate-900 text-white p-6 rounded-3xl shadow-xl border border-slate-800 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <span className="px-3 py-1 rounded-full bg-teal-500/20 text-teal-300 border border-teal-400/30 text-xs font-bold">
            Frontend Developer Handoff • HappyPet v1.0
          </span>
          <h1 className="text-2xl font-black mt-2">
            UI/UX სპეციფიკაცია, Design Tokens & კომპონენტების არქიტექტურა
          </h1>
          <p className="text-xs text-slate-400 mt-1">
            მომზადებულია <strong className="text-white">https://github.com/Gogitajgarkava/HappyPet.git</strong> (.NET 8 Clean Architecture) ბექენდთან პირდაპირი ინტეგრაციისთვის.
          </p>
        </div>
        <button
          onClick={() => setViewportMode('mobile')}
          className="px-4 py-2.5 rounded-xl bg-teal-600 hover:bg-teal-500 text-white text-xs font-bold whitespace-nowrap self-start md:self-center"
        >
          ← მობილურ პროტოტიპზე დაბრუნება
        </button>
      </div>

      {/* Deliverable 1: User Flow & Navigation Map */}
      <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-sm space-y-4">
        <h2 className="text-lg font-extrabold text-slate-900 flex items-center gap-2">
          <Layers className="w-5 h-5 text-teal-600" />
          1. User Flow & Navigation Map (მომხმარებლის ნაკადი)
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-3 text-xs">
          <div className="p-4 rounded-2xl bg-slate-50 border border-slate-200 space-y-1.5">
            <span className="px-2 py-0.5 rounded bg-teal-100 text-teal-800 font-bold text-[10px]">
              ნაბიჯი 1: Auth & RBAC
            </span>
            <h4 className="font-extrabold text-slate-900">JWT ავტორიზაცია</h4>
            <p className="text-slate-600 leading-relaxed">
              მომხმარებელი ირჩევს როლს (`Owner`, `Vet`, `Shelter`), გადის 11-ნიშნა პირადი ნომრის ვალიდაციას. `AuthContext` ინახავს JWT ტოკენს (7 დღე).
            </p>
          </div>
          <div className="p-4 rounded-2xl bg-slate-50 border border-slate-200 space-y-1.5">
            <span className="px-2 py-0.5 rounded bg-indigo-100 text-indigo-800 font-bold text-[10px]">
              ნაბიჯი 2: Pet CRUD & AIRS
            </span>
            <h4 className="font-extrabold text-slate-900">ცხოველის რეგისტრაცია</h4>
            <p className="text-slate-600 leading-relaxed">
              `POST /api/pets` ამოწმებს 15-ნიშნა ISO მიკროჩიპის უნიკალურობას PostgreSQL-ში და აბამს თბილისის AMA-ს საყელოს ნომერს.
            </p>
          </div>
          <div className="p-4 rounded-2xl bg-slate-50 border border-slate-200 space-y-1.5">
            <span className="px-2 py-0.5 rounded bg-emerald-100 text-emerald-800 font-bold text-[10px]">
              ნაბიჯი 3: ვეტ-პასპორტი
            </span>
            <h4 className="font-extrabold text-slate-900">QR სკანი და ვაქცინაცია</h4>
            <p className="text-slate-600 leading-relaxed">
              ვეტერინარი სკანირებს QR-ს ან ეძებს 15-ნიშნა ჩიპით, ამატებს ცოფის ვაქცინაციას ან სტერილიზაციის ჩანაწერს.
            </p>
          </div>
          <div className="p-4 rounded-2xl bg-rose-50 border border-rose-200 space-y-1.5">
            <span className="px-2 py-0.5 rounded bg-rose-200 text-rose-900 font-bold text-[10px]">
              ნაბიჯი 4: SOS Broadcast
            </span>
            <h4 className="font-extrabold text-rose-950">დაკარგვის განგაში</h4>
            <p className="text-rose-800 leading-relaxed">
              1 კლიკით ივსება ჩიპის კოდი, ირჩევა თბილისის უბანი და იგზავნება Push-შეტყობინება რადარზე.
            </p>
          </div>
        </div>
      </div>

      {/* Deliverable 2: Design Tokens (Colors, Typography, Touch Targets) */}
      <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-sm space-y-4">
        <h2 className="text-lg font-extrabold text-slate-900 flex items-center gap-2">
          <Sparkles className="w-5 h-5 text-teal-600" />
          2. Design System Tokens (Tailwind CSS Configuration)
        </h2>
        <div className="grid grid-cols-2 sm:grid-cols-5 gap-3 text-xs">
          <div className="p-3 rounded-2xl border border-slate-200">
            <div className="h-12 rounded-xl bg-teal-600 mb-2 shadow-inner" />
            <span className="font-extrabold block">Primary (Warm Teal)</span>
            <code className="text-[11px] text-slate-500">#0D9488 • bg-teal-600</code>
          </div>
          <div className="p-3 rounded-2xl border border-slate-200">
            <div className="h-12 rounded-xl bg-indigo-600 mb-2 shadow-inner" />
            <span className="font-extrabold block">Secondary (Indigo)</span>
            <code className="text-[11px] text-slate-500">#4F46E5 • bg-indigo-600</code>
          </div>
          <div className="p-3 rounded-2xl border border-slate-200">
            <div className="h-12 rounded-xl bg-emerald-500 mb-2 shadow-inner" />
            <span className="font-extrabold block">AIRS Verified</span>
            <code className="text-[11px] text-slate-500">#10B981 • bg-emerald-500</code>
          </div>
          <div className="p-3 rounded-2xl border border-slate-200">
            <div className="h-12 rounded-xl bg-rose-600 mb-2 shadow-inner" />
            <span className="font-extrabold block">SOS Emergency</span>
            <code className="text-[11px] text-slate-500">#E11D48 • bg-rose-600</code>
          </div>
          <div className="p-3 rounded-2xl border border-slate-200">
            <div className="h-12 rounded-xl bg-slate-900 mb-2 shadow-inner" />
            <span className="font-extrabold block">Neutral Surface</span>
            <code className="text-[11px] text-slate-500">#0F172A • bg-slate-900</code>
          </div>
        </div>
      </div>

      {/* Deliverable 3: Zod Validation Schema & TanStack Query Architecture */}
      <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-sm space-y-4">
        <h2 className="text-lg font-extrabold text-slate-900 flex items-center gap-2">
          <Code2 className="w-5 h-5 text-indigo-600" />
          3. Frontend Architecture Blueprint (Zod + TanStack Query + .NET 8 API)
        </h2>
        <pre className="bg-slate-950 text-teal-300 p-4 rounded-2xl text-xs font-mono overflow-x-auto leading-relaxed">
{`// src/schemas/petSchema.ts (Matches HappyPet.Application FluentValidation)
import { z } from 'zod';

export const petRegistrationSchema = z.object({
  name: z.string().min(2, 'ცხოველის სახელი სავალდებულოა'),
  petType: z.enum(['Dog', 'Cat', 'Other']),
  gender: z.enum(['Male', 'Female']),
  breed: z.string().min(2, 'ჯიშის მითითება სავალდებულოა'),
  microchipCode: z
    .string()
    .regex(/^\\d{15}$/, 'AIRS სტანდარტი: მიკროჩიპი უნდა შედგებოდეს ზუსტად 15 ციფრისგან'),
  collarNumber: z.string().optional(),
  visualCharacteristics: z.string().min(10, 'აღწერეთ ვიზუალური ნიშნები'),
  isSterilized: z.boolean(),
});

// src/hooks/usePetsQuery.ts (TanStack Query v5 — No Redux)
export const usePets = () => useQuery({
  queryKey: ['pets', 'my-list'],
  queryFn: () => apiClient.get('/api/pets').then(res => res.data),
});`}
        </pre>
      </div>
    </div>
  );

  // ============================================================================
  // 6. MAIN RENDER & DEVICE FRAME WRAPPER
  // ============================================================================

  return (
    <div className="min-h-screen bg-slate-100 text-slate-900 flex flex-col font-sans selection:bg-teal-600 selection:text-white">
      {/* --- TOP DEVELOPER & INSPECTOR BAR --- */}
      <header className="sticky top-0 z-40 bg-slate-900 text-white border-b border-slate-800 px-3 sm:px-6 py-2.5">
        <div className="max-w-7xl mx-auto flex flex-wrap items-center justify-between gap-2">
          {/* Brand & Repo Status */}
          <div className="flex items-center gap-2.5">
            <div className="w-8 h-8 rounded-xl bg-teal-500 flex items-center justify-center font-black text-slate-950">
              <PawPrint className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <span className="font-black text-sm tracking-tight">HappyPet</span>
                <span className="text-[10px] px-2 py-0.5 rounded-full bg-teal-500/20 text-teal-300 border border-teal-500/30 font-mono">
                  .NET 8 + React/Vite
                </span>
              </div>
              <p className="text-[10px] text-slate-400 hidden sm:block">
                UI/UX Handoff Prototype • AIRS / Tbilisi AMA Integrated
              </p>
            </div>
          </div>

          {/* Role Switcher (RBAC Inspector) */}
          <div className="flex items-center gap-1 bg-slate-800 p-1 rounded-xl border border-slate-700">
            <span className="text-[10px] text-slate-400 px-2 hidden md:inline">RBAC როლი:</span>
            {['Owner', 'Vet', 'Shelter'].map((r) => (
              <button
                key={r}
                onClick={() => handleRoleChange(r)}
                className={`px-2.5 py-1 rounded-lg text-[11px] font-bold transition ${
                  userRole === r
                    ? 'bg-teal-500 text-slate-950 shadow-sm'
                    : 'text-slate-300 hover:text-white'
                }`}
              >
                {r === 'Owner' ? 'მესაკუთრე' : r === 'Vet' ? 'ვეტ-კლინიკა' : 'თავშესაფარი'}
              </button>
            ))}
          </div>

          {/* Viewport Mode Switcher */}
          <div className="flex items-center gap-1.5">
            <button
              onClick={() => setViewportMode('mobile')}
              className={`px-2.5 py-1.5 rounded-xl text-xs font-bold flex items-center gap-1 transition ${
                viewportMode === 'mobile'
                  ? 'bg-white text-slate-900'
                  : 'bg-slate-800 text-slate-300 hover:bg-slate-700'
              }`}
            >
              <Smartphone className="w-3.5 h-3.5" />
              <span className="hidden sm:inline">Mobile Frame</span>
            </button>
            <button
              onClick={() => setViewportMode('full')}
              className={`px-2.5 py-1.5 rounded-xl text-xs font-bold flex items-center gap-1 transition ${
                viewportMode === 'full'
                  ? 'bg-white text-slate-900'
                  : 'bg-slate-800 text-slate-300 hover:bg-slate-700'
              }`}
            >
              <Monitor className="w-3.5 h-3.5" />
              <span className="hidden sm:inline">Full Width</span>
            </button>
            <button
              onClick={() => setViewportMode('specs')}
              className={`px-3 py-1.5 rounded-xl text-xs font-bold flex items-center gap-1 transition ${
                viewportMode === 'specs'
                  ? 'bg-indigo-500 text-white'
                  : 'bg-indigo-600/30 border border-indigo-400/30 text-indigo-200 hover:bg-indigo-600/50'
              }`}
            >
              <Code2 className="w-3.5 h-3.5" />
              <span>Dev Specs & Code</span>
            </button>
          </div>
        </div>
      </header>

      {/* --- STACKED SONNER-STYLE TOAST NOTIFICATIONS --- */}
      <div className="fixed top-16 right-4 z-50 flex flex-col gap-2 max-w-sm w-full pointer-events-none px-4 sm:px-0">
        {toasts.map((t) => (
          <div
            key={t.id}
            className={`pointer-events-auto p-3.5 rounded-2xl shadow-xl border backdrop-blur-md flex items-start gap-3 animate-fadeIn ${
              t.type === 'sos'
                ? 'bg-rose-600 text-white border-rose-500'
                : t.type === 'error'
                ? 'bg-rose-950 text-white border-rose-700'
                : 'bg-slate-900/95 text-white border-slate-700'
            }`}
          >
            <CheckCircle2 className="w-5 h-5 text-emerald-400 flex-shrink-0 mt-0.5" />
            <div className="text-xs">
              <strong className="font-extrabold block">{t.title}</strong>
              <span className="text-slate-300 text-[11px]">{t.description}</span>
            </div>
          </div>
        ))}
      </div>

      {/* --- MAIN CONTENT BODY --- */}
      {viewportMode === 'specs' ? (
        renderDeveloperHandoffSpecs()
      ) : (
        <main className="flex-1 flex justify-center items-start py-0 sm:py-6 px-0 sm:px-4">
          <div
            className={`w-full bg-slate-50 flex flex-col overflow-hidden transition-all duration-300 ${
              viewportMode === 'mobile'
                ? 'max-w-[430px] sm:rounded-[42px] sm:border-[10px] sm:border-slate-900 sm:shadow-2xl min-h-[840px] max-h-[880px]'
                : 'max-w-3xl rounded-3xl border border-slate-200 shadow-lg min-h-[820px]'
            }`}
          >
            {/* Simulated Mobile Status Bar */}
            <div className="bg-white px-5 pt-3 pb-2.5 border-b border-slate-200/80 flex items-center justify-between sticky top-0 z-20">
              <div className="flex items-center gap-2">
                <div className="w-8 h-8 rounded-xl bg-teal-600 text-white flex items-center justify-center shadow-sm">
                  <PawPrint className="w-4 h-4" />
                </div>
                <div>
                  <div className="flex items-center gap-1.5">
                    <span className="font-black text-sm text-slate-900">HappyPet</span>
                    <span className="text-[10px] font-bold px-1.5 py-0.5 rounded bg-emerald-100 text-emerald-800">
                      AIRS
                    </span>
                  </div>
                  <span className="text-[10px] text-slate-400 block leading-none">
                    თბილისი • {userRole}
                  </span>
                </div>
              </div>

              <div className="flex items-center gap-1.5">
                <button
                  onClick={() => setIsSosModalOpen(true)}
                  className="px-2.5 py-1.5 rounded-xl bg-rose-50 hover:bg-rose-100 text-rose-600 border border-rose-200 text-[11px] font-extrabold flex items-center gap-1"
                >
                  <AlertTriangle className="w-3.5 h-3.5" /> SOS
                </button>
                <button
                  onClick={() => setActiveTab('auth')}
                  className="p-2 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700"
                  title="JWT Auth / პროფილი"
                >
                  <User className="w-4 h-4" />
                </button>
              </div>
            </div>

            {/* Scrollable Screen Viewport */}
            <div className="flex-1 overflow-y-auto px-4 pt-4">
              {activeTab === 'home' && renderHomeScreen()}
              {activeTab === 'passport' && renderPassportScreen()}
              {activeTab === 'radar' && renderRadarScreen()}
              {activeTab === 'auth' && renderAuthScreen()}
            </div>

            {/* Mobile Bottom Navigation Bar (48x48px min touch targets) */}
            <nav className="bg-white border-t border-slate-200 px-3 py-2 grid grid-cols-4 gap-1 sticky bottom-0 z-20">
              {[
                { id: 'home', label: 'მთავარი', icon: Heart },
                { id: 'passport', label: 'ვეტ-პასპორტი', icon: ShieldCheck },
                { id: 'radar', label: 'SOS რადარი', icon: MapPin, badge: sosAlerts.length },
                { id: 'auth', label: 'პროფილი / JWT', icon: User }
              ].map((item) => {
                const Icon = item.icon;
                const isActive = activeTab === item.id;
                return (
                  <button
                    key={item.id}
                    onClick={() => setActiveTab(item.id)}
                    className={`min-h-[48px] rounded-2xl flex flex-col items-center justify-center relative transition ${
                      isActive
                        ? 'text-teal-600 bg-teal-50/80 font-extrabold'
                        : 'text-slate-500 hover:text-slate-800 font-medium'
                    }`}
                  >
                    <Icon className="w-5 h-5" />
                    <span className="text-[10px] mt-0.5">{item.label}</span>
                    {item.badge && (
                      <span className="absolute top-1 right-4 px-1.5 py-0.2 rounded-full text-[9px] font-black bg-rose-600 text-white">
                        {item.badge}
                      </span>
                    )}
                  </button>
                );
              })}
            </nav>
          </div>
        </main>
      )}

      {/* --- MODALS RENDERING --- */}
      {isSosModalOpen && <SosModal />}
      {isPetCrudModalOpen && <PetCrudModal />}
      {isQrModalOpen && <QrPassportModal />}
      {isChipLookupModalOpen && <ChipLookupModal />}
      {isVetRecordModalOpen && <VetRecordModal />}

      {/* Delete Pet Confirmation Modal (No native confirm()!) */}
      {petToDelete && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/70 backdrop-blur-sm p-4">
          <div className="bg-white max-w-sm w-full rounded-3xl p-5 space-y-4 text-center shadow-2xl">
            <div className="w-12 h-12 rounded-2xl bg-rose-100 text-rose-600 flex items-center justify-center mx-auto">
              <Trash2 className="w-6 h-6" />
            </div>
            <h3 className="font-extrabold text-base text-slate-900">
              წაიშალოს {petToDelete.name} პროფილიდან?
            </h3>
            <p className="text-xs text-slate-500">
              მოქმედება გააგზავნის <code className="font-mono">DELETE /api/pets/{petToDelete.id}</code> მოთხოვნას. AIRS არქივში ჩიპის ისტორია შენარჩუნდება.
            </p>
            <div className="flex gap-2">
              <button
                onClick={() => setPetToDelete(null)}
                className="w-1/2 py-2.5 rounded-xl border border-slate-300 text-xs font-bold text-slate-700"
              >
                გაუქმება
              </button>
              <button
                onClick={() => {
                  setPets((prev) => prev.filter((p) => p.id !== petToDelete.id));
                  addToast('წაიშალა', `${petToDelete.name} წაიშალა თქვენი სიიდან.`, 'info');
                  setPetToDelete(null);
                }}
                className="w-1/2 py-2.5 rounded-xl bg-rose-600 text-white text-xs font-bold"
              >
                დიახ, წაშლა
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Sighting / Contact Owner Modal */}
      {activeSightingAlert && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/70 backdrop-blur-sm p-4">
          <div className="bg-white max-w-md w-full rounded-3xl p-5 space-y-4 shadow-2xl border border-slate-200">
            <div className="flex items-center justify-between">
              <span className="px-2.5 py-1 rounded-full bg-rose-100 text-rose-700 text-xs font-extrabold">
                📍 SOS კონტაქტი და კოორდინატების გადაცემა
              </span>
              <button
                onClick={() => setActiveSightingAlert(null)}
                className="p-1.5 rounded-full bg-slate-100"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            <div className="flex items-center gap-3">
              <img
                src={activeSightingAlert.avatarUrl}
                alt={activeSightingAlert.petName}
                className="w-14 h-14 rounded-2xl object-cover"
              />
              <div>
                <h4 className="font-extrabold text-base text-slate-900">
                  {activeSightingAlert.petName}
                </h4>
                <p className="text-xs text-slate-600">
                  პატრონი: <strong>{activeSightingAlert.ownerName}</strong>
                </p>
                <p className="text-xs font-mono text-teal-700">
                  ჩიპი: {activeSightingAlert.microchipCode}
                </p>
              </div>
            </div>

            <div className="p-3 bg-slate-50 rounded-2xl border border-slate-200 text-xs space-y-1">
              <span className="text-slate-400 block text-[10px]">საგანგებო ნომერი:</span>
              <div className="flex items-center justify-between">
                <span className="font-extrabold text-sm text-slate-900">
                  {activeSightingAlert.contactPhone}
                </span>
                <button
                  onClick={() => {
                    addToast(
                      'ზარი ხორციელდება...',
                      `უკავშირდებით ${activeSightingAlert.ownerName}-ს (${activeSightingAlert.contactPhone})`,
                      'success'
                    );
                    setActiveSightingAlert(null);
                  }}
                  className="px-3 py-1.5 bg-teal-600 text-white rounded-xl text-xs font-bold flex items-center gap-1"
                >
                  <Phone className="w-3.5 h-3.5" /> დარეკვა
                </button>
              </div>
            </div>

            <button
              onClick={() => {
                setSosAlerts((prev) =>
                  prev.map((a) =>
                    a.id === activeSightingAlert.id
                      ? { ...a, sightingsCount: a.sightingsCount + 1, lastSeenTime: '1 წუთის წინ' }
                      : a
                  )
                );
                addToast(
                  'GPS მდებარეობა გაიგზავნა!',
                  `პატრონს (${activeSightingAlert.ownerName}) გაეგზავნა თქვენი მიმდინარე კოორდინატები.`,
                  'success'
                );
                setActiveSightingAlert(null);
              }}
              className="w-full py-3 bg-rose-600 hover:bg-rose-700 text-white rounded-xl text-xs font-extrabold shadow-md flex items-center justify-center gap-1.5"
            >
              <MapPin className="w-4 h-4" /> დავაფიქსირე! ჩემი GPS მდებარეობის გაგზავნა
            </button>
          </div>
        </div>
      )}
    </div>
  );
}