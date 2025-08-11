'use client';

import { usePathname } from 'next/navigation';
import Link from 'next/link';
import { signOut } from 'next-auth/react';
import { 
  Home, 
  FolderOpen, 
  Search, 
  MessageSquare, 
  BookOpen,
  Settings,
  LogOut,
  ChevronLeft,
  ChevronRight,
  User
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';

interface SidebarProps {
  open: boolean;
  user?: {
    name?: string | null;
    email?: string | null;
    image?: string | null;
  };
}

const navigation = [
  { name: 'Dashboard', href: '/dashboard', icon: Home },
  { name: 'Repositories', href: '/repositories', icon: FolderOpen },
  { name: 'Search', href: '/search', icon: Search },
  { name: 'Chat', href: '/chat', icon: MessageSquare },
  { name: 'Documentation', href: '/documentation', icon: BookOpen },
];

const secondaryNavigation = [
  { name: 'Settings', href: '/settings', icon: Settings },
];

export function Sidebar({ open, user }: SidebarProps) {
  const pathname = usePathname();

  const handleSignOut = () => {
    signOut({ callbackUrl: '/auth/signin' });
  };

  return (
    <div
      className={cn(
        'flex flex-col h-full bg-card border-r transition-all duration-300',
        open ? 'w-64' : 'w-16'
      )}
    >
      {/* Logo and brand */}
      <div className="flex items-center px-4 py-4 border-b">
        <div className="flex items-center">
          <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
            <svg
              className="w-5 h-5 text-primary-foreground"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
          </div>
          {open && (
            <div className="ml-3">
              <h1 className="text-lg font-semibold text-foreground">Archie</h1>
              <p className="text-xs text-muted-foreground">Repository Analysis</p>
            </div>
          )}
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-2 py-4 space-y-1">
        {navigation.map((item) => {
          const isActive = pathname === item.href;
          return (
            <Link
              key={item.name}
              href={item.href}
              className={cn(
                'group flex items-center px-2 py-2 text-sm font-medium rounded-md transition-colors',
                isActive
                  ? 'bg-primary text-primary-foreground'
                  : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
                !open && 'justify-center'
              )}
            >
              <item.icon
                className={cn(
                  'flex-shrink-0 w-5 h-5',
                  open ? 'mr-3' : ''
                )}
                aria-hidden="true"
              />
              {open && item.name}
            </Link>
          );
        })}
      </nav>

      {/* Secondary navigation */}
      <div className="px-2 py-4 border-t">
        {secondaryNavigation.map((item) => {
          const isActive = pathname === item.href;
          return (
            <Link
              key={item.name}
              href={item.href}
              className={cn(
                'group flex items-center px-2 py-2 text-sm font-medium rounded-md transition-colors',
                isActive
                  ? 'bg-primary text-primary-foreground'
                  : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
                !open && 'justify-center'
              )}
            >
              <item.icon
                className={cn(
                  'flex-shrink-0 w-5 h-5',
                  open ? 'mr-3' : ''
                )}
                aria-hidden="true"
              />
              {open && item.name}
            </Link>
          );
        })}
      </div>

      {/* User profile */}
      <div className="px-2 py-4 border-t">
        {open ? (
          <div className="space-y-3">
            <div className="flex items-center px-2 py-2">
              <Avatar className="w-8 h-8">
                <AvatarImage src={user?.image || ''} alt={user?.name || ''} />
                <AvatarFallback>
                  {user?.name?.split(' ').map(n => n[0]).join('').toUpperCase() || 'U'}
                </AvatarFallback>
              </Avatar>
              <div className="ml-3 flex-1 min-w-0">
                <p className="text-sm font-medium text-foreground truncate">
                  {user?.name}
                </p>
                <p className="text-xs text-muted-foreground truncate">
                  {user?.email}
                </p>
              </div>
            </div>
            <Button
              onClick={handleSignOut}
              variant="outline"
              size="sm"
              className="w-full justify-start"
            >
              <LogOut className="w-4 h-4 mr-2" />
              Sign Out
            </Button>
          </div>
        ) : (
          <div className="flex flex-col items-center space-y-2">
            <Avatar className="w-8 h-8">
              <AvatarImage src={user?.image || ''} alt={user?.name || ''} />
              <AvatarFallback>
                {user?.name?.split(' ').map(n => n[0]).join('').toUpperCase() || 'U'}
              </AvatarFallback>
            </Avatar>
            <Button
              onClick={handleSignOut}
              variant="outline"
              size="sm"
              className="p-2"
            >
              <LogOut className="w-4 h-4" />
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}