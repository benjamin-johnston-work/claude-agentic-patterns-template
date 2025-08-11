'use client';

import { Menu, Bell, Settings, Search } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { ThemeToggle } from '@/components/navigation/ThemeToggle';

interface HeaderProps {
  onMobileMenuOpen: () => void;
  onSidebarToggle: () => void;
  user?: {
    name?: string | null;
    email?: string | null;
    image?: string | null;
  };
}

export function Header({ onMobileMenuOpen, onSidebarToggle, user }: HeaderProps) {
  return (
    <div className="bg-background border-b">
      <div className="px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Left section */}
          <div className="flex items-center">
            <Button
              variant="ghost"
              size="sm"
              className="lg:hidden"
              onClick={onMobileMenuOpen}
            >
              <Menu className="h-5 w-5" />
              <span className="sr-only">Open sidebar</span>
            </Button>
            
            <Button
              variant="ghost"
              size="sm"
              className="hidden lg:flex"
              onClick={onSidebarToggle}
            >
              <Menu className="h-5 w-5" />
              <span className="sr-only">Toggle sidebar</span>
            </Button>
          </div>

          {/* Center section - Search */}
          <div className="flex-1 max-w-md mx-4">
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <Search className="h-4 w-4 text-muted-foreground" />
              </div>
              <Input
                type="search"
                placeholder="Search repositories, documentation..."
                className="pl-10 pr-4"
              />
            </div>
          </div>

          {/* Right section */}
          <div className="flex items-center space-x-3">
            <ThemeToggle />
            
            <Button variant="ghost" size="sm" className="relative">
              <Bell className="h-5 w-5" />
              <span className="sr-only">View notifications</span>
              {/* Notification badge */}
              <span className="absolute -top-1 -right-1 h-3 w-3 bg-destructive rounded-full text-xs flex items-center justify-center text-destructive-foreground">
                2
              </span>
            </Button>

            <Button variant="ghost" size="sm">
              <Settings className="h-5 w-5" />
              <span className="sr-only">Settings</span>
            </Button>

            <Avatar className="h-8 w-8">
              <AvatarImage src={user?.image || ''} alt={user?.name || ''} />
              <AvatarFallback className="text-xs">
                {user?.name?.split(' ').map(n => n[0]).join('').toUpperCase() || 'U'}
              </AvatarFallback>
            </Avatar>
          </div>
        </div>
      </div>
    </div>
  );
}